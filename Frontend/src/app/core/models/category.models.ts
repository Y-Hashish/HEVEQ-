export type CategoryType = 'Service' | 'Marketplace'

export interface Category {
  id: number
  name: string
  slug: string
  type: CategoryType
  parentId: number | null
}

export type CategoryDto = Category

