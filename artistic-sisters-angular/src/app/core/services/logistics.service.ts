import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { TrackingInfo, DeliveryAgent, DeliveryAssignmentDto } from '../models/models';

@Injectable({ providedIn: 'root' })
export class LogisticsService {
  private readonly BASE = 'http://localhost:5000/api/logistics';
  constructor(private http: HttpClient) {}

  track(orderId: string): Observable<TrackingInfo> {
    return this.http.get<TrackingInfo>(`${this.BASE}/track/${orderId}`);
  }

  getAgents(): Observable<DeliveryAgent[]> {
    return this.http.get<DeliveryAgent[]>(`${this.BASE}/agents`);
  }

  updateStatus(orderId: string, status: number): Observable<any> {
    return this.http.put<any>(`${this.BASE}/status`, { orderId, newStatus: status });
  }

  pushGPS(orderId: string, latitude: number, longitude: number): Observable<any> {
    return this.http.post<any>(`${this.BASE}/gps`, { orderId, latitude, longitude });
  }

  getAssignments(): Observable<DeliveryAssignmentDto[]> {
    return this.http.get<DeliveryAssignmentDto[]>(`${this.BASE}/assignments`);
  }

  getAgentAssignments(agentId: string): Observable<DeliveryAssignmentDto[]> {
    return this.http.get<DeliveryAssignmentDto[]>(`${this.BASE}/assignments/agent/${agentId}`);
  }

  addVehicle(vehicleInfo: { registrationNumber: string; vehicleType: string }): Observable<any> {
    return this.http.post<any>(`${this.BASE}/vehicles`, vehicleInfo);
  }

  addAgent(agentInfo: { name: string; phone: string; email: string }): Observable<any> {
    return this.http.post<any>(`${this.BASE}/agents`, agentInfo);
  }
}
