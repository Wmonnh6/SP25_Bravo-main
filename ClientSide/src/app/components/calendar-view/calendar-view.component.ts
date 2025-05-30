import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FullCalendarModule } from '@fullcalendar/angular';
import { CalendarOptions } from '@fullcalendar/core'; // useful for typechecking
import dayGridPlugin from '@fullcalendar/daygrid';
import { CalendarViewService } from '../../services/calendar-view.service';
import { MessageService } from 'primeng/api';

@Component({
    selector: 'app-calendar-view',
    imports: [CommonModule, FullCalendarModule],
    templateUrl: './calendar-view.component.html',
    styleUrl: './calendar-view.component.scss'
})
export class CalendarViewComponent {

    calendarOptions: CalendarOptions = {
        initialView: 'dayGridMonth',
        plugins: [dayGridPlugin],
        height: '80vh',
        buttonText: {
            today: 'Current Month'
        }
    };

    constructor(
        private calendarViewService: CalendarViewService,
        private messageService: MessageService) {
        calendarViewService.getAllEmployees().subscribe({
            next: (res) => {
                console.debug(res);

                if (res.success) {
                    this.calendarOptions = {
                        ...this.calendarOptions,
                        events: res.data.map(x => ({ title: x.name, date: x.date }))
                    };
                }
            },
            error: (err) => {
                console.error('Error fetching the employees time off requests: ', err)
            }
        });
    }

    // Function to show error message
    showError(msg: string) {
        this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: msg
        });
    }
}
