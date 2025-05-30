export class TimeEntryRequest
{
    private _id: number;
    private _taskId: number;
    private _userId: number;
    private _hours: number;
    private _comment: string;
    private _date: Date;

    constructor(
        id: number,
        taskId: number,
        userId : number,
        hours : number,
        comment : string,
        date : Date
    ) {
        this._id = id;
        this._taskId = taskId;
        this._userId = userId;
        this._hours = hours;
        this._comment = comment;
        this._date = date;
    }

    get Id() {
        return this._id;
    }

    set Id(value: number) {
        this._id = value;
    }

    get TaskId() {
        return this._taskId;
    }

    set TaskId(value: number) {
        this._taskId = value;
    }

    get UserId() {
        return this._userId;
    }

    set UserId(value: number) {
        this._userId = value;
    }

    get Hours() {
        return this._hours;
    }

    set Hours(value: number) {
        this._hours = value;
    }

    get Comment() {
        return this._comment;
    }

    set Comment(value: string) {
        this._comment = value;
    }

    get Date() {
        return this._date;
    }

    set Date(value: Date) {
        this._date = value;
    }
}