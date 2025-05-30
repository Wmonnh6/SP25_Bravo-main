export class GetTimeOffRequestsRequest {
    constructor(
        public userId?: number,
        public requestStatus?: string,
        public startDate?: Date,
        public endDate?: Date
    ) { }
}
