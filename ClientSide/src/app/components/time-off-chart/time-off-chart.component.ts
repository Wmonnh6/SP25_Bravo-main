import { Component } from '@angular/core';
import { TimeOffSummaryService } from '../../services/time-off-summary.service';
import { MessageService } from 'primeng/api';
import { ChartModule } from 'primeng/chart';
import { ChartData, ChartOptions } from 'chart.js';
import { TimeOffSummary } from '../../models/timeOffSummary';
import { CommonModule } from '@angular/common';
import { subMonths, addMonths } from 'date-fns';
import { ButtonModule } from 'primeng/button';

@Component({
  selector: 'app-time-off-chart',
  imports: [ChartModule, CommonModule, ButtonModule],
  templateUrl: './time-off-chart.component.html',
  styleUrl: './time-off-chart.component.scss'
})
export class TimeOffChartComponent {
  chartData: ChartData<'bar'>;
  chartOptions: ChartOptions<'bar'>;
  currentMonth: Date = new Date();
  currentDate: Date = new Date();

  constructor(
    private timeOffSummaryService: TimeOffSummaryService,
    private messageService: MessageService
  ) { }

  ngOnInit(): void {
    this.fetchTimeOffData(this.currentMonth);
  }

  fetchTimeOffData(selectedDate: Date): void {
    console.log('Fetching data for:', selectedDate);
    this.timeOffSummaryService.getTimeOffSummary(selectedDate).subscribe({
      next: (res) => {
        if (res.success && res.data && res.data.length > 0) {
          this.formatChartData(res.data);
        } else {
          this.chartData = null;
          this.showError(res.message);
        }
      },
      error: (err) => {
        console.error('Error fetching time-off summary:', err);
        this.showError('Failed to retrieve time-off summary.');
      }
    });
  }

  formatChartData(data: TimeOffSummary[]): void {
    console.log("Received Data: ", data);
    const labels = data.map(emp => emp.userName); // Format employee names
    const timeOffHours = data.map(emp => emp.totalHours); // Extract total hours

     // Function to determine the bar color based on the time off hours
    const getColor = (hours: number): string => {
      if (hours >= 100) {
        return 'red';
      } else if (hours >= 80) {
        return 'orange';
      } else if (hours >= 40) {
        return 'yellow';
      } else {
        return 'green';
      }
    };

    const monthLabel = `${(this.currentMonth.getMonth() + 1).toString().padStart(2, '0')}-${this.currentMonth.getFullYear()}`;

    this.chartData = {
      labels: labels,
      datasets: [
        {
          label: `Total Time Off Hours  -  ${monthLabel}`,
          data: timeOffHours,
          backgroundColor: timeOffHours.map(hours => getColor(hours)),
          borderColor: 'rgb(109, 116, 116)',
          borderWidth: 1
        }
      ]
    };

    this.chartOptions = {
      responsive: true,
      maintainAspectRatio: false,
      plugins: {
        legend: {
          display: true,
          position: 'top',
          onClick: () => null,
          labels: {
            usePointStyle: true,
            boxWidth: 0
          }
        },
        tooltip: {
          callbacks: {
            // Custom label for the tooltip
            label: (tooltipItem) => {
              //const datasetLabel = tooltipItem.dataset.label || '';
              const value = tooltipItem.raw; // The actual data value for the bar
              
              
              return `Time-Off:  ${value} hours`; // Custom Display for tooltip
            }
          }
        }
      },
      scales: {
        x: {
          ticks: { autoSkip: false, maxRotation: 0, minRotation: 0 },
          title: {
            display: true,
            text: 'Employee Name',
            padding: 10 // Add some padding between the labels and the title
          }
        },
        y: {
          beginAtZero: true,
          max: 120,
          title: { display: true, text: 'Hours' },
          ticks: {
            stepSize: 10,
            maxTicksLimit: 200,
            autoSkip: false
          }
        }
      }
    };
  }

  previousMonth(): void {
    this.currentMonth = subMonths(this.currentMonth, 1);
    this.fetchTimeOffData(this.currentMonth);
  }

  nextMonth(): void {
    this.currentMonth = addMonths(this.currentMonth, 1);
    this.fetchTimeOffData(this.currentMonth);
  }

  goToCurrentMonth(): void {
    this.currentMonth = new Date(); // Reset to current date
    this.fetchTimeOffData(this.currentMonth); // Fetch the data for the current month
  }

  isDifferentMonth(): boolean {
    return this.currentMonth.getFullYear() !== this.currentDate.getFullYear() ||
           this.currentMonth.getMonth() !== this.currentDate.getMonth();
  }

  showError(msg: string): void {
    this.messageService.add({ severity: 'error', summary: 'Error', detail: msg });
  }
}

