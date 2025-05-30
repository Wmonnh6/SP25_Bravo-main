import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TimeOffManagementComponent } from './time-off-management.component';

describe('TimeOffManagementComponent', () => {
  let component: TimeOffManagementComponent;
  let fixture: ComponentFixture<TimeOffManagementComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TimeOffManagementComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(TimeOffManagementComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
