import { Task } from "./task";
import { TimeOffRequestDTO } from "./timeOffRequestDTO";
import { UserDto } from "./userDto";

export class MyTimeEntry {

    constructor(
        public id: number,
        public userId: number,
        public user: UserDto,
        public comment: string,
        public hours: number,
        public task: Task,
        public date: Date,
        public timeOffRequest: TimeOffRequestDTO) { }
}