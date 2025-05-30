# SP25_Bravo-main

This is the team repository for Bravo

## Project
This application allows users to log their working hours for managerial tracking.

Administrators can invite employees to create accounts by providing a unique email address. The employee will then receive an invitation via email. Upon clicking the invitation link, the employee must complete the account creation form by providing a unique username, first name, last name, and password. Users will also have the ability to reset or change their password.

Once the account is created, users can log in by the email and password. Logged-in users can update their information and record their working hours by clicking "Add a Time Entry." When adding a time entry, users must select the day, the number of hours worked, the task (from a list of available tasks), and provide a comment. Employees can view their time entries on a weekly basis in a table format. They can also update or delete any existing time entry.

Administrators can create tasks that employees can select when logging their working hours. They can also deactivate tasks, making them unavailable for future use. Additionally, administrators have the ability to view, delete, and update time entries for other employees.

# Project Name
My Time Entry

# Project Description
The project solves the problem of efficiently tracking and managing employee work hours. It provides a platform where employees can log their hours worked, allowing for accurate and easily accessible time records. Administrators can track and manage employee time entries, ensuring accountability and transparency.

The application allows administrators to invite employees to create accounts, log in, and update their information. Employees can record their hours worked by adding time entries, which include the day, hours worked, the task performed, and any additional comments. This information is organized on a weekly basis for easy review.

Additionally, administrators have the ability to create and deactivate tasks, and they can view, update, or delete time entries from employees. This system ensures smooth time management, making it easier for both employees and administrators to monitor work hours and ensure accurate payroll or project tracking.

Team
Team details follow

495 Project Leader
Majed Alasemi

394 Students
Wessley Monnin
Aaron Voymas

294 Students
Ava Barrick
Jack Gifford

Set Up
Details on how to set up the project follow.

Install the following tools:

SQL Server 2022 the developer edition.
https://www.microsoft.com/en-us/sql-server/sql-server-downloads

SQL Server Management Studio (SSMS)
https://learn.microsoft.com/en-us/sql/ssms/download-sql-server-management-studio-ssms?view=sql-server-ver16
Click the download link as the following picture:
alt text

Visual Studio Community 2022
https://visualstudio.microsoft.com/vs/pricing/?tab=free-tools
Options to select:
Workloads: While installing it you need to check the ASP.NET and web development options as minimum requirements.
Individual components: you need to make sure .NET 8.0 Runtime is checked.

Visual Studio Code
https://visualstudio.microsoft.com/vs/pricing/?tab=free-tools
Make sure you select the “Add to PATH” option.
Install the “Angular Language Service” extension in Visual Studio Code.

Node.js (v22.13.0 LTS)
https://nodejs.org/en/download

Git
https://git-scm.com/downloads/win
When installing, the default options should be good for our project.

We are going to use Angular for this project.
More information about Angular:
https://angular.dev/overview
https://angular.dev/cli
We are going to use PrimeNg for the front-end with Angular https://primeng.org/autocomplete

Running the Back-End:
Navigate to the ServerSide folder and open the "ServerSide.sln" file.
It should open by Visual Studio.
Inside Visual Studio, click the IIS Express as showing in this image: alt text or Debug -> Start Debugging.
After that, the back-end project should be running.

Running the Front-End:
Open the command line and run: npm install -g @angular/cli
Open the client-side folder by Visual Studio Code and type “npm install” and then enter.
When the installation is complete, type “ng s” and enter to run the Angular project.
You should be able to open localhost:4200 in the browser and see the Angular app is running.
