export class AddTimeEntryRequest {
    constructor(
        public UserId: number,
        public Hours: number,
        public MyTimeEntryTaskId: number,
        public Comment: String,
        public Date: Date) { }
}