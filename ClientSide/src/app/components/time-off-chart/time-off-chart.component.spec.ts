import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TimeOffChartComponent } from './time-off-chart.component';

describe('TimeOffChartComponent', () => {
  let component: TimeOffChartComponent;
  let fixture: ComponentFixture<TimeOffChartComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TimeOffChartComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(TimeOffChartComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
