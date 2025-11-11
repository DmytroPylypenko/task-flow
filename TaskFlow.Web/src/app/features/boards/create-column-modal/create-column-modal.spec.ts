import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CreateColumnModalComponent as CreateColumnModal } from './create-column-modal';

describe('CreateColumnModal', () => {
  let component: CreateColumnModal;
  let fixture: ComponentFixture<CreateColumnModal>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CreateColumnModal]
    })
    .compileComponents();

    fixture = TestBed.createComponent(CreateColumnModal);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
