using ServerSide.DatabaseContext;
using ServerSide.DTOs;
using ServerSide.Managers.TimeEntryManager;
using ServerSide.Mapper;
using ServerSide.Models.Entities;
using ServerSide.Requests;

namespace UnitTest
{
    public class TimeEntryTests : IClassFixture<TestSetup>
    {
        private readonly ITimeEntryManager _unitUnderTest;
        private readonly ApplicationDbContext _dbContext;

        // test arguments
        private const string DATE_WITH_NO_ENTRIES = "1970-1-1";
        private const string DATE_WITH_TIME_ENTRIES = "2025-3-30";
        private const string DATE_WITH_TIME_ENTRIES_DIFF_WEEK = "2025-3-22"; // this one is to test that only the entries from one week are obtained

        public TimeEntryTests(TestSetup setup)
        {
            _unitUnderTest = setup.GetService<ITimeEntryManager>();
            _dbContext = setup.GetService<ApplicationDbContext>();

            PopulateTestData();
        }

        /*
         * Tests:
         *  1. Adding time entries
         *  2. Updating time entries
         *  3. Deleting time entries
         *  4. Getting time entries
         */

        [Theory]
        [InlineData(1, DATE_WITH_TIME_ENTRIES, 1)] // user1 only gets user1's time entries
        [InlineData(2, DATE_WITH_TIME_ENTRIES, 2)] // user2 only gets user2's time entries
        public async void GetTimeEntries_ValidUserId_DateWithEntries_ReturnsListOfEntries(int userId, DateTime date, int numEntries)
        {
            var result = await _unitUnderTest.GetUserTimeEntriesAsync(userId, date);

            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.IsType<List<TimeEntryDTO>>(result.Data);
            Assert.Equal(numEntries, result.Data.Count);
			Assert.Equal("Time entries retrieved successfully.", result.Message);
		}

        [Theory]
		[InlineData(0, DATE_WITH_NO_ENTRIES)] // invalid user, date with no associated time entries
        [InlineData(0, DATE_WITH_TIME_ENTRIES)] // invalid user, date that has time entries for other users
        public async void GetTimeEntries_InvalidUserId_ReturnsUnsuccessfulResult(int userId, DateTime date)
        {
            var result = await _unitUnderTest.GetUserTimeEntriesAsync(userId, date);

            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.Equal("No time entries found for this user.", result.Message);
        }

        [Theory]
        [InlineData(1, DATE_WITH_TIME_ENTRIES, 1)] // user1 will have the 1 entry for this week
        //[InlineData(1, DATE_WITH_TIME_ENTRIES_DIFF_WEEK, 0)] // but user1 doesn't have any entries for this week; when there are no entries for a given week the data is null
        [InlineData(2, DATE_WITH_TIME_ENTRIES, 2)] // user2 has 2 entries this week
        [InlineData(2, DATE_WITH_TIME_ENTRIES_DIFF_WEEK, 1)] // and only 1 time entry this other week
        public async void GetTimeEntries_ValidUserId_DateWithEntries_ReturnsOnlyEntriesForThatWeek(int userId, DateTime date, int numEntries)
        {
            var result = await _unitUnderTest.GetUserTimeEntriesAsync(userId, date);

            Assert.NotNull(result.Data); // this assertion here makes sure any further assertions will not have a null data
            Assert.Equal(numEntries, result.Data.Count);
            result.Data.ForEach(te => Assert.Equal(TimeEntryManager.GetStartOfWeek(te.Date), TimeEntryManager.GetStartOfWeek(date))); // this shows that the time entries have the same start of week
            result.Data.ForEach(te => Assert.Equal(TimeEntryManager.GetStartOfWeek(te.Date).AddDays(7),
                                                    TimeEntryManager.GetStartOfWeek(date).AddDays(7))); // validates that the time entries have the same end of week
        }

        // Test you can add a time entry to a non-closed week
        [Theory]
        [InlineData(1, 2, DATE_WITH_TIME_ENTRIES)] // user1 adds task2 to an open week
        [InlineData(2, 1, DATE_WITH_TIME_ENTRIES_DIFF_WEEK)] // user2 adds task1 to an open week
        public async void AddTimeEntry_OpenWeek_ReturnsTimeEntryDTO(int userId, int taskId, DateTime date)
        {
            var ter = new TimeEntryRequest
            {
                UserId = userId,
                TaskId = taskId,
                Date = date,
                Hours = 1 // the request model requires at least 1 hour
            };

            var result = await _unitUnderTest.AddTimeEntryAsync(ter);

            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.IsType<TimeEntryDTO>(result.Data);
            Assert.NotNull(result.Data.User);
            Assert.Equal(userId, result.Data.User.Id);
            Assert.NotNull(result.Data.Task);
            Assert.Equal(taskId, result.Data.Task.Id);

			// cleanup entries added to the database
			var entryToRemove = await _dbContext.TimeEntries.FindAsync(result.Data.Id);
			_dbContext.TimeEntries.Remove(entryToRemove);
			await _dbContext.SaveChangesAsync(); // cleanup to avoid other tests failing
		}

        [Theory]
        [InlineData(DATE_WITH_TIME_ENTRIES)] // test that it can add a new time entry, then close the week and try again expect failure
        public async void AddTimeEntry_ClosedWeek_FailsToAddNewEntry(DateTime date)
        {
            var ter = new TimeEntryRequest
            {
                UserId = 1,
                TaskId = 1,
                Date = date,
                Hours = 1,
                Comment = "Added before week closed"
            };

            var resultOpen = await _unitUnderTest.AddTimeEntryAsync(ter);

            Assert.True(resultOpen.Success);
            Assert.NotNull(resultOpen.Data);

            // close the week
            var startOfWeek = TimeEntryManager.GetStartOfWeek(date);
			var cw = new ClosedWeek
            {
                DateClosed = startOfWeek
            };
            _dbContext.ClosedWeeks.Add(cw);
            await _dbContext.SaveChangesAsync(); // close the week

            // try adding a new entry
            var resultClosed = await _unitUnderTest.AddTimeEntryAsync(ter);

            Assert.False(resultClosed.Success);
            Assert.Null(resultClosed.Data);

            // cleanup entries added to the database
            _dbContext.ClosedWeeks.Remove(cw);
            // TODO: remove the new time entry to avoid future tests failing
            var entryToRemove = _dbContext.TimeEntries.First(te => te.Id == resultOpen.Data.Id);
            _dbContext.TimeEntries.Remove(entryToRemove);
            await _dbContext.SaveChangesAsync(); // cleanup to avoid other tests failing
        }

        [Theory]
        [InlineData(0)] // userid for a non-existent user that was added during interception between the frontend and backend
        public async void AddTimeEntry_InTransit_RequestModified_UserIdInvalid_RequestFails(int userId)
        {
			var ter = new TimeEntryRequest
			{
				UserId = userId,
				TaskId = 1,
				Date = DateTime.Parse(DATE_WITH_TIME_ENTRIES),
				Hours = 1 // the request model requires at least 1 hour
			};

            var result = await _unitUnderTest.AddTimeEntryAsync(ter);

            Assert.False(result.Success);
            Assert.Null(result.Data); // the result will not contain data
            Assert.Equal("User not found.", result.Message);
		}

		[Theory]
        [InlineData(0)] // taskid for a non-existent task
		public async void AddTimeEntry_InTransit_RequestModified_TaskIdInvalid_RequestFails(int taskId)
		{
			var ter = new TimeEntryRequest
			{
				UserId = 1,
				TaskId = taskId,
				Date = DateTime.Parse(DATE_WITH_TIME_ENTRIES),
				Hours = 1 // the request model requires at least 1 hour
			};

			var result = await _unitUnderTest.AddTimeEntryAsync(ter);

			Assert.False(result.Success);
			Assert.Null(result.Data); // the result will not contain data
			Assert.Equal("Task not found.", result.Message);
		}

		// update time entry
		// 2. request to update in closed week fails
		[Theory]
        [InlineData(DATE_WITH_TIME_ENTRIES)]
		public async void UpdateTimeEntry_WeekClosed_UpdateFails(DateTime date)
        {
            var ter = new TimeEntryRequest
            {
                Id = 1,
                UserId = 1,
                TaskId = 1,
                Hours = 1,
                Date = date
            };

			var startOfWeek = TimeEntryManager.GetStartOfWeek(date);
			var cw = new ClosedWeek
            {
                DateClosed = startOfWeek
            };

            _dbContext.ClosedWeeks.Add(cw);
            await _dbContext.SaveChangesAsync();

            var result = await _unitUnderTest.UpdateTimeEntryAsync(ter);

            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.Equal("Selected week is closed.", result.Message);

            _dbContext.ClosedWeeks.Remove(cw);
            await _dbContext.SaveChangesAsync();
        }
        // 3. request with changed user id fails
        [Theory]
        [InlineData(1, 2)]
        [InlineData(2, 1)]
        public async void UpdateTimeEntry_AttemptedUserChange_UpdateFails(int timeEntryId, int userId)
        {
            var ter = new TimeEntryRequest
            {
                Id = timeEntryId,
                UserId = userId,
                TaskId = 1,
                Hours = 1,
                Date = DateTime.Parse(DATE_WITH_TIME_ENTRIES)
            };

            var result = await _unitUnderTest.UpdateTimeEntryAsync(ter);

			Assert.False(result.Success);
			Assert.Null(result.Data);
			Assert.Equal("Malformed request.", result.Message);
		}
        // 8. good request succeeds
        [Theory]
        [InlineData(1,1,2,2,"updated comment",DATE_WITH_TIME_ENTRIES_DIFF_WEEK)] // change time entry 1's information
        [InlineData(2,2,2,5,null,DATE_WITH_TIME_ENTRIES_DIFF_WEEK)] // change entry 2's information
        [InlineData(3,2,2,3,"",DATE_WITH_TIME_ENTRIES)] // change time entry 3's information
        [InlineData(4,2,1,12,"newer comment",DATE_WITH_TIME_ENTRIES)] // change time entry 4's information
        public async void UpdateTimeEntry_UpdateAllFieldsExceptUserId_ReturnsUpdatedTimeEntry(int entryId, int userId, int taskId, int hours, string comment, DateTime date)
        {
            var ter = new TimeEntryRequest
            {
                Id = entryId,
                UserId = userId,
                TaskId = taskId,
                Hours = hours,
                Comment = comment,
                Date = date
            };
            var te = await _dbContext.TimeEntries.FindAsync(entryId);
			Assert.NotNull(te);
			var origTe = te.ToDTO();
            Assert.NotNull(origTe.User);
            Assert.NotNull(origTe.Task);

            var result = await _unitUnderTest.UpdateTimeEntryAsync(ter);

            // verify the request succeeded
			Assert.True(result.Success);
			Assert.NotNull(result.Data);
			Assert.IsType<TimeEntryDTO>(result.Data);
            // verify the elements were updated to the new values
            Assert.NotNull(result.Data.Task);
            Assert.Equal(taskId, result.Data.Task.Id);
            Assert.Equal(hours, result.Data.Hours);
            Assert.Equal(comment, result.Data.Comment);
            Assert.Equal(date, result.Data.Date);

            // cleanup the changes
            var cleanupTer = new TimeEntryRequest
            {
                Id = origTe.Id,
                UserId = origTe.User.Id,
                TaskId = origTe.Task.Id,
                Hours = origTe.Hours,
                Comment = origTe.Comment,
                Date = origTe.Date
            };

            await _unitUnderTest.UpdateTimeEntryAsync(cleanupTer);
		}

        // delete time entries
        // 1. user cannot delete other user's time entries
        [Theory]
		[InlineData(1, 2, false)]
		[InlineData(2,3,false)]
        [InlineData(3,1,false)] // even a user like 1 can't delete another user's entry if not an admin (note they technically are but I'm faking that they aren't)
		[InlineData(4, 3, false)] // user 3 has no time entries so they are safe to use for this test
		public async void DeleteTimeEntry_NonAdminOtherUser_DeleteFails(int entryId, int userId, bool isAdmin)
        {
            var dter = new DeleteTimeEntryRequest
            {
                TimeEntryId = entryId
            };

            var result = await _unitUnderTest.DeleteTimeEntryAsync(dter, userId, isAdmin);

            Assert.False(result.Success);
            Assert.Equal("You can only delete your own time entries.", result.Message);

            // show the entry is still there
            var te = await _dbContext.TimeEntries.FindAsync(entryId);

            Assert.NotNull(te);
        }
        // 2. user can delete their own time entries
        [Theory]
        [InlineData(1,1,false)]
        [InlineData(2,2,false)]
        [InlineData(3,2,false)]
        [InlineData(4,2,false)]
        public async void DeleteTimeEntry_NonAdminSameUser_DeleteSucceeds(int entryid, int userId, bool isAdmin)
        {
            var dter = new DeleteTimeEntryRequest
            {
                TimeEntryId = entryid
            };

            // get the entry to add it back in during cleanup
            var te = await _dbContext.TimeEntries.FindAsync(entryid);
            Assert.NotNull(te);
            var teDTO = te.ToDTO();
            var cleanupTe = new TimeEntries
            {
                Id = te.Id,
                UserId = te.UserId,
                User = te.User,
                Comment = te.Comment,
                Hours = te.Hours,
                MyTimeEntryTaskId = te.MyTimeEntryTaskId,
                MyTimeEntryTask = te.MyTimeEntryTask,
                Date = te.Date
            };

            // act
            var result = await _unitUnderTest.DeleteTimeEntryAsync(dter, userId, isAdmin);

            // assert
            Assert.True(result.Success);
            Assert.Equal(entryid, teDTO.Id); // the entry to delete has the same id as the one deleted
            Assert.Equal("Time entry deleted successfully.", result.Message);

            // cleanup and add the entry back in
            // verify that they have the same entry id as originally
            _dbContext.TimeEntries.Add(cleanupTe);
            await _dbContext.SaveChangesAsync();
        }
        // 3. admins can delete other user's time entries
        [Theory]
        [InlineData(2,1)] // user 1 as admin can delete entry 2 which is user 2's
        [InlineData(3,3)] // user 3 who has no entries, as admin, can delete entries for user 2
        [InlineData(1,2)] // user 2 as admin can delete entry for user 1
        public async void DeleteTimeEntry_AdminOtherUser_DeleteSucceeds(int entryId, int userId)
        {
			var dter = new DeleteTimeEntryRequest
			{
				TimeEntryId = entryId
			};

			// get the entry to add it back in during cleanup
			var te = await _dbContext.TimeEntries.FindAsync(entryId);
			Assert.NotNull(te);
			var teDTO = te.ToDTO();
			var cleanupTe = new TimeEntries
			{
				Id = te.Id,
				UserId = te.UserId,
				User = te.User,
				Comment = te.Comment,
				Hours = te.Hours,
				MyTimeEntryTaskId = te.MyTimeEntryTaskId,
				MyTimeEntryTask = te.MyTimeEntryTask,
				Date = te.Date
			};

			// act
			var result = await _unitUnderTest.DeleteAnyTimeEntryAsync(dter);

			// assert
			Assert.True(result.Success);
			Assert.Equal(entryId, teDTO.Id); // the entry to delete has the same id as the one deleted
			Assert.Equal("Time entry deleted successfully.", result.Message);

			// cleanup and add the entry back in
			// verify that they have the same entry id as originally
			_dbContext.TimeEntries.Add(cleanupTe);
			await _dbContext.SaveChangesAsync();
		}
        // 4. admins can delete their own time entries
        [Theory]
        // user 1's entries, as admin
        [InlineData(1,1)]
        // user 2's entries, as admin
        [InlineData(2,2)]
        [InlineData(3,2)]
        [InlineData(4,2)]
        public async void DeleteTimeEntry_AdminSelf_DeleteSucceeds(int entryId, int userId)
        {
			var dter = new DeleteTimeEntryRequest
			{
				TimeEntryId = entryId
			};

			// get the entry to add it back in during cleanup
			var te = await _dbContext.TimeEntries.FindAsync(entryId);
			Assert.NotNull(te);
			var teDTO = te.ToDTO();
			var cleanupTe = new TimeEntries
			{
				Id = te.Id,
				UserId = te.UserId,
				User = te.User,
				Comment = te.Comment,
				Hours = te.Hours,
				MyTimeEntryTaskId = te.MyTimeEntryTaskId,
				MyTimeEntryTask = te.MyTimeEntryTask,
				Date = te.Date
			};

			// act
			var result = await _unitUnderTest.DeleteAnyTimeEntryAsync(dter);

			// assert
			Assert.True(result.Success);
			Assert.Equal(entryId, teDTO.Id); // the entry to delete has the same id as the one deleted
			Assert.Equal("Time entry deleted successfully.", result.Message);

			// cleanup and add the entry back in
			// verify that they have the same entry id as originally
			_dbContext.TimeEntries.Add(cleanupTe);
			await _dbContext.SaveChangesAsync();
		}
        // 5. user cannot delete time entry from closed week
        [Theory]
        // entries based upon what week they are on, non admin users
        [InlineData(1,1,false,DATE_WITH_TIME_ENTRIES)]
        [InlineData(2,2,false,DATE_WITH_TIME_ENTRIES)]
        [InlineData(3,2,false,DATE_WITH_TIME_ENTRIES)]
        [InlineData(4,2,false, DATE_WITH_TIME_ENTRIES_DIFF_WEEK)]
        public async void DeleteTimeEntry_NonAdminClosedWeek_DeleteFails(int entryId, int userId, bool isAdmin, DateTime date)
        {
			var dter = new DeleteTimeEntryRequest
			{
				TimeEntryId = entryId
			};

			var startOfWeek = TimeEntryManager.GetStartOfWeek(date);
			var cw = new ClosedWeek
            {
                DateClosed = startOfWeek
            };

            _dbContext.ClosedWeeks.Add(cw);
            await _dbContext.SaveChangesAsync();

			// act
			var result = await _unitUnderTest.DeleteTimeEntryAsync(dter, userId, isAdmin);

			// assert
			Assert.False(result.Success);
			Assert.Equal("Week is closed.", result.Message);

            // cleanup
            _dbContext.ClosedWeeks.Remove(cw);
			await _dbContext.SaveChangesAsync();
		}
		// 6. admin cannot delete time entry from closed week
		[Theory]
		// entries based upon what week they are on, admin users
		[InlineData(1, DATE_WITH_TIME_ENTRIES)]
		[InlineData(2, DATE_WITH_TIME_ENTRIES)]
		[InlineData(3, DATE_WITH_TIME_ENTRIES)]
		[InlineData(4, DATE_WITH_TIME_ENTRIES_DIFF_WEEK)]
		public async void DeleteTimeEntry_AdminClosedWeek_DeleteFails(int entryId, DateTime date)
        {
			var dter = new DeleteTimeEntryRequest
			{
				TimeEntryId = entryId
			};

			var startOfWeek = TimeEntryManager.GetStartOfWeek(date);
			var cw = new ClosedWeek
			{
				DateClosed = startOfWeek
			};

			_dbContext.ClosedWeeks.Add(cw);
			await _dbContext.SaveChangesAsync();

			// act
			var result = await _unitUnderTest.DeleteAnyTimeEntryAsync(dter);

			// assert
			Assert.False(result.Success);
			Assert.Equal("Week is closed.", result.Message);

			// cleanup
			_dbContext.ClosedWeeks.Remove(cw);
			await _dbContext.SaveChangesAsync();
		}
		// 7. cannot delete time entry that does not exist
		[Theory]
		// entry doesn't exist, non admins
		[InlineData(0,1,false)]
		public async void DeleteTimeEntry_NonExistent_DeleteFails(int entryId, int userId, bool isAdmin)
        {
			var dter = new DeleteTimeEntryRequest
			{
				TimeEntryId = entryId
			};

			// act
			var result = await _unitUnderTest.DeleteTimeEntryAsync(dter, userId, isAdmin);

			// assert
			Assert.False(result.Success);
			Assert.Equal("Couldn't find the time entry.", result.Message);
		}
        // 8. cannot delete time entry that does not exist, admins
		[Theory]
		// entry doesn't exist, non admins
		[InlineData(0)]
		public async void DeleteTimeEntry_NonExistent_Admin_DeleteFails(int entryId)
        {
			var dter = new DeleteTimeEntryRequest
			{
				TimeEntryId = entryId
			};

			// act
			var result = await _unitUnderTest.DeleteAnyTimeEntryAsync(dter);

			// assert
			Assert.False(result.Success);
			Assert.Equal("Couldn't find the time entry.", result.Message);
		}

		private void PopulateTestData() 
        {
            // need at least 2 good users and tasks
            // users
            User userWithTimeEntries1 = new User
            {
                FirstName = "User1",
                LastName = "WithTimeEntries",
                Password = "Password1!",
                Email = "Email@email.com",
                IsAdmin = true
            };
            User userWithTimeEntries2 = new User
            {
				FirstName = "User2",
				LastName = "WithTimeEntries",
				Password = "Password1!",
				Email = "Email@email.com",
				IsAdmin = false
			};
            User userWithNoTimeEntries = new User
            {
				FirstName = "User3",
				LastName = "WithNoTimeEntries",
				Password = "Password1!",
				Email = "Email@email.com",
				IsAdmin = false
			};
            _dbContext.Users.AddRange([userWithTimeEntries1, userWithTimeEntries2, userWithNoTimeEntries]);

            // tasks
            MyTimeEntryTask task1 = new MyTimeEntryTask
            {
                Name = "Do Stuff",
                IsActive = true,
                IsTimeOff = false
            };
            MyTimeEntryTask task2 = new MyTimeEntryTask
            {
                Name = "Do Nothing",
                IsActive = true,
                IsTimeOff = true
            };
            _dbContext.MyTimeEntryTasks.AddRange([task1, task2]);

            // time entries
            TimeEntries te1 = new TimeEntries
            {
                User = userWithTimeEntries1,
                UserId = userWithTimeEntries1.Id,
                Hours = 1,
                MyTimeEntryTask = task1,
                MyTimeEntryTaskId = task1.Id,
                Date = DateTime.Parse(DATE_WITH_TIME_ENTRIES)
            };
            TimeEntries te2 = new TimeEntries
            {
                User = userWithTimeEntries2,
                UserId = userWithTimeEntries2.Id,
                Hours = 1,
                Comment = "Normal time entry",
                MyTimeEntryTask = task1,
                MyTimeEntryTaskId = task1.Id,
                Date = DateTime.Parse(DATE_WITH_TIME_ENTRIES)
            };
            TimeEntries te3 = new TimeEntries
            {
				User = userWithTimeEntries2,
				UserId = userWithTimeEntries2.Id,
				Hours = 0,
                Comment = "Time entry with 0 hours",
				MyTimeEntryTask = task2,
				MyTimeEntryTaskId = task2.Id,
				Date = DateTime.Parse(DATE_WITH_TIME_ENTRIES)
			};
            TimeEntries te4 = new TimeEntries
            {
				User = userWithTimeEntries2,
				UserId = userWithTimeEntries2.Id,
				Hours = 1,
                Comment = "Time entry on a different week",
				MyTimeEntryTask = task1,
				MyTimeEntryTaskId = task1.Id,
				Date = DateTime.Parse(DATE_WITH_TIME_ENTRIES_DIFF_WEEK)
			};
            _dbContext.TimeEntries.AddRange([te1, te2, te3, te4]);

            _dbContext.SaveChanges();
        }

        private void CleanUpTestData()
        {
        }
	}

}
