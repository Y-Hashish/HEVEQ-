import { ComponentFixture, TestBed } from '@angular/core/testing'
import { CreateListing } from './create-listing'

describe('CreateListing', () => {
  let component: CreateListing
  let fixture: ComponentFixture<CreateListing>

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CreateListing]
    }).compileComponents()

    fixture = TestBed.createComponent(CreateListing)
    component = fixture.componentInstance
    fixture.detectChanges()
  })

  it('should initialize a listing form with required controls', () => {
    expect(component.form.get('title')).toBeTruthy()
    expect(component.form.get('categoryId')).toBeTruthy()
    expect(component.form.get('transactionMethod')).toBeTruthy()
    expect(component.form.get('price')).toBeTruthy()
    expect(component.form.get('description')).toBeTruthy()
  })
})
