import { TestBed } from '@angular/core/testing';

import { TimeOffManagementService } from './time-off-management.service';

describe('TimeOffManagementService', () => {
  let service: TimeOffManagementService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(TimeOffManagementService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
