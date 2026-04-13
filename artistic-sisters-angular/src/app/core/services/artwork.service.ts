import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Artwork, AddArtworkRequest } from '../models/models';

@Injectable({ providedIn: 'root' })
export class ArtworkService {
  private readonly BASE = 'http://localhost:5000/api/artworks';

  constructor(private http: HttpClient) {}

  getAll(artworkType?: string, medium?: string): Observable<Artwork[]> {
    let params = new HttpParams();
    if (artworkType) params = params.set('ArtworkType', artworkType);
    if (medium)      params = params.set('Medium', medium);
    return this.http.get<Artwork[]>(this.BASE, { params });
  }

  getById(id: string): Observable<Artwork> {
    return this.http.get<Artwork>(`${this.BASE}/${id}`);
  }

  add(req: AddArtworkRequest): Observable<any> {
    return this.http.post<any>(this.BASE, req);
  }

  updateStock(id: string, quantity: number): Observable<any> {
    return this.http.patch<any>(`${this.BASE}/${id}/stock`, { quantity });
  }

  deleteArtwork(id: string): Observable<any> {
    return this.http.delete<any>(`${this.BASE}/${id}`);
  }

  toggleComingSoon(id: string, isComingSoon: boolean): Observable<any> {
    return this.http.patch<any>(`${this.BASE}/${id}/coming-soon`, { isComingSoon });
  }
}
