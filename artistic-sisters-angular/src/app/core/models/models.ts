// All TypeScript interfaces matching the backend exactly

export interface AuthResult {
  success: boolean;
  message: string;
  customerId?: string;
  token?: string;
  role?: string;
  name?: string;
  email?: string;
}

export interface RegisterRequest {
  name: string;
  email: string;
  password: string;
  phone: string;
  address: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface Artwork {
  id: string;
  title: string;
  description: string;
  price: number;
  artworkType: string;
  medium: string;
  dimensions: string;
  artworkCode: string;
  imageUrl: string;
  availableQuantity: number;
  isCustomizable: boolean;
  isAvailable: boolean;
  isComingSoon: boolean;
  estimatedCompletionDays: number;
}

export interface AddArtworkRequest {
  title: string;
  description: string;
  price: number;
  artworkType: string;
  medium: string;
  dimensions: string;
  artworkCode: string;
  imageUrl: string;
  availableQuantity: number;
  isCustomizable: boolean;
  estimatedCompletionDays: number;
}

export interface PlaceOrderRequest {
  customerId: string;
  customerName: string;
  customerEmail: string;
  shippingAddress: string;
  type: OrderType;
  paymentMode: PaymentMode;
  items: OrderItem[];
}

export interface OrderItem {
  artworkId: string;
  artworkTitle: string;
  quantity: number;
  unitPrice: number;
}

export interface CommissionRequest {
  customerId: string;
  customerName: string;
  customerEmail: string;
  artworkType: string;
  medium: string;
  size: string;
  referencePhotoUrl: string;
  specialInstructions: string;
  budgetMin: number;
  budgetMax: number;
}

export interface OrderResult {
  success: boolean;
  message: string;
  orderId?: string;
}

export interface PaymentStatus {
  orderId: string;
  amount: number;
  status: string;
  paymentMode: string;
  createdAt: Date;
  confirmedAt?: Date;
}

export interface SagaState {
  orderId: string;
  state: string;
  paymentLocked: boolean;
  agentAssigned: boolean;
  startedAt: Date;
  completedAt?: Date;
  failureReason?: string;
}

export interface TrackingInfo {
  orderId: string;
  latitude: number;
  longitude: number;
  updatedAt: Date;
}

export interface DeliveryAgent {
  id: string;
  name: string;
  phone: string;
  email: string;
  status: string;
  currentOrderId?: string;
}

export interface NotificationLog {
  eventType: string;
  recipientEmail: string;
  subject: string;
  isSuccess: boolean;
  errorMessage?: string;
  sentAt: Date;
}

export enum OrderType {
  ReadyMade = 0,
  CustomCommission = 1
}

export enum PaymentMode {
  Card = 0,
  CashOnDelivery = 1,
  BankTransfer = 2
}

export enum OrderStatus {
  Pending = 0,
  Confirmed = 1,
  InProduction = 2,
  ReadyForDelivery = 3,
  OutForDelivery = 4,
  Delivered = 5,
  Cancelled = 6
}

export interface CustomerOrderDto {
  id: string;
  type: OrderType;
  status: OrderStatus;
  totalAmount: number;
  paymentMode: PaymentMode;
  placedAt: Date;
  artworkType?: string;
  medium?: string;
  items: CustomerOrderItemDto[];
}

export interface CustomerOrderItemDto {
  artworkId: string;
  artworkTitle: string;
  quantity: number;
  unitPrice: number;
}

export interface AdminOrderDto extends CustomerOrderDto {
  customerId: string;
}

export interface DeliveryAssignmentDto {
  id: string;
  orderId: string;
  status: number;
  statusText: string;
  assignedAt: Date;
  slaDeadline: Date;
  lastLatitude?: number;
  lastLongitude?: number;
  lastGPSUpdate?: Date;
  agent: { id: string; name: string; phone: string; email: string; };
  vehicle: { registrationNumber: string; vehicleType: string; };
}
