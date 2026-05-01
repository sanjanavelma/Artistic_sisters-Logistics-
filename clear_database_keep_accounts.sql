-- ============================================================
-- Artistic Sisters - Clear All Records (Safe Reset Script)
-- Keeps:
--   Admin:          sanjanavelma27@gmail    (IdentityDB.Customers)
--   Delivery Agent: sanjanavelma2745@gmail.com (LogisticsDB.Agents)
-- ============================================================
-- Run this in SSMS connected to localhost\SQLEXPRESS
-- ============================================================

PRINT '=== Starting Artistic Sisters Database Cleanup ===';
PRINT '';

-- ============================================================
-- 1. ORDER DB  (clears all order data)
-- ============================================================
USE OrderDB;
PRINT 'Clearing OrderDB...';

DELETE FROM OutboxMessages;
DELETE FROM OrderItems;
DELETE FROM CustomCommissionOrders;
DELETE FROM Orders;

PRINT '  ✓ OrderDB cleared.';

-- ============================================================
-- 2. PAYMENT DB  (clears all payment records)
-- ============================================================
USE PaymentDB;
PRINT 'Clearing PaymentDB...';

DELETE FROM DispatchSagas;
DELETE FROM Payments;

PRINT '  ✓ PaymentDB cleared.';

-- ============================================================
-- 3. LOGISTICS DB  (clear assignments & vehicles; keep delivery agent)
-- ============================================================
USE LogisticsDB;
PRINT 'Clearing LogisticsDB (keeping sanjanavelma2745@gmail.com)...';

DELETE FROM Assignments;
DELETE FROM Vehicles;

-- Delete all delivery agents EXCEPT the one to keep
DELETE FROM Agents
WHERE Email NOT IN ('sanjanavelma2745@gmail.com');

PRINT '  ✓ LogisticsDB cleared (delivery agent account preserved).';

-- ============================================================
-- 4. ARTWORK DB  (clears all artworks and notify-me subscriptions)
-- ============================================================
USE ArtworkDB;
PRINT 'Clearing ArtworkDB...';

DELETE FROM NotifyMeSubscriptions;
DELETE FROM Artworks;

PRINT '  ✓ ArtworkDB cleared.';

-- ============================================================
-- 5. NOTIFICATION DB  (clear logs; keep email templates)
-- ============================================================
USE NotificationDB;
PRINT 'Clearing NotificationDB logs (keeping EmailTemplates)...';

DELETE FROM NotificationLogs;

PRINT '  ✓ NotificationDB logs cleared.';

-- ============================================================
-- 6. IDENTITY DB  (clear customers; keep admin account)
-- ============================================================
USE IdentityDB;
PRINT 'Clearing IdentityDB (keeping sanjanavelma27@gmail)...';

-- Delete all customers EXCEPT the admin account
DELETE FROM Customers
WHERE Email NOT IN ('sanjanavelma27@gmail');

PRINT '  ✓ IdentityDB cleared (admin account preserved).';

-- ============================================================
-- 7. HANGFIRE DB  (clear all Hangfire job history)
-- ============================================================
USE HangfireDB;
PRINT 'Clearing HangfireDB...';

-- Hangfire tables (delete in correct dependency order)
IF OBJECT_ID('HangFire.JobParameter')      IS NOT NULL DELETE FROM [HangFire].[JobParameter];
IF OBJECT_ID('HangFire.JobQueue')          IS NOT NULL DELETE FROM [HangFire].[JobQueue];
IF OBJECT_ID('HangFire.State')             IS NOT NULL DELETE FROM [HangFire].[State];
IF OBJECT_ID('HangFire.Job')               IS NOT NULL DELETE FROM [HangFire].[Job];
IF OBJECT_ID('HangFire.Counter')           IS NOT NULL DELETE FROM [HangFire].[Counter];
IF OBJECT_ID('HangFire.AggregatedCounter') IS NOT NULL DELETE FROM [HangFire].[AggregatedCounter];
IF OBJECT_ID('HangFire.Hash')              IS NOT NULL DELETE FROM [HangFire].[Hash];
IF OBJECT_ID('HangFire.List')              IS NOT NULL DELETE FROM [HangFire].[List];
IF OBJECT_ID('HangFire.Set')               IS NOT NULL DELETE FROM [HangFire].[Set];
IF OBJECT_ID('HangFire.Server')            IS NOT NULL DELETE FROM [HangFire].[Server];

PRINT '  ✓ HangfireDB cleared.';

-- ============================================================
-- DONE
-- ============================================================
PRINT '';
PRINT '=== Cleanup Complete! ===';
PRINT 'Admin account (sanjanavelma27@gmail) preserved in IdentityDB.';
PRINT 'Delivery agent account (sanjanavelma2745@gmail.com) preserved in LogisticsDB.';
