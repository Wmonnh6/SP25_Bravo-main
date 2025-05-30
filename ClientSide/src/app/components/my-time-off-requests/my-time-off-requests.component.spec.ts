import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MyTimeOffRequestsComponent } from './my-time-off-requests.component';

describe('MyTimeOffRequestsComponent', () => {
  let component: MyTimeOffRequestsComponent;
  let fixture: ComponentFixture<MyTimeOffRequestsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [MyTimeOffRequestsComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(MyTimeOffRequestsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
