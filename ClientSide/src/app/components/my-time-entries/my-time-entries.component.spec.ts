import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MyTimeEntriesComponent } from './my-time-entries.component';

describe('MyTimeEntriesComponent', () => {
  let component: MyTimeEntriesComponent;
  let fixture: ComponentFixture<MyTimeEntriesComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [MyTimeEntriesComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(MyTimeEntriesComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
