export class TimeOffRequestManagementDTO {
    constructor(
        public id: number,
        public hours: number,
        public date: Date,
        public comment: string,
        public timeOffRequest: TimeOffRequestDTO,
        public user: TimeOffUserDTO
    ) { }
}

export class TimeOffUserDTO {
    constructor(
        public id: number,
        public firstName: string,
        public lastName: string
    ) { }
}

export class TimeOffRequestDTO {
    constructor(
        public id: number,
        public Status: string
    ) { }
}
