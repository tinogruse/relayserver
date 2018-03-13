export interface PageRequest {
  searchText?: string;
  sortDirection?: 'asc' | 'desc' | '';
  sortField?: string;
  pageIndex: number;
  pageSize: number;
}
