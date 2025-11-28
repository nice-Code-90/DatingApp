export type GeoLocation = {
  type: string;
  coordinates: [number, number];
};

export type Member = {
  id: string;
  age: number;
  imageUrl?: string;
  displayName: string;
  created: string;
  lastActive: string;
  gender: string;
  description?: string;
  city: string | null;
  country: string | null;
  photos: Photo[];
};
export type Photo = {
  id: number;
  url: string;
  isMain: boolean;
  isApproved: boolean;
};
export type EditableMember = {
  displayName: string;
  description?: string;
  city: string;
  country: string;
};

export class MemberParams {
  gender?: string;
  minAge = 18;
  maxAge = 100;
  pageNumber = 1;
  pageSize = 10;
  orderBy = 'lastActive';
  distance?: number | undefined;
  unit = 'km';
}
