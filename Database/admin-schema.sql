-- =====================================================================
-- SculptFlow Admin Portal — admin-only tables.
--
-- These live in the SAME database as the main app (Supabase Postgres) but are owned by this repo.
-- The main app never reads them, and clinic staff accounts (identity_users) can never sign in to the
-- admin portal: admins are a completely separate user list.
--
-- Idempotent: safe to run more than once. Append new blocks at the bottom, never edit applied ones.
-- =====================================================================

-- 2026-10-05 — admin users and the admin audit log
create schema if not exists admin;

create table if not exists admin.admin_users (
    id              uuid primary key default gen_random_uuid(),
    email           text not null,
    full_name       text not null,
    password_hash   text not null,
    is_active       boolean not null default true,
    failed_logins   int not null default 0,
    locked_until    timestamptz null,
    last_login_at   timestamptz null,
    created_at      timestamptz not null default now(),
    updated_at      timestamptz not null default now()
);

create unique index if not exists ux_admin_users_email on admin.admin_users (lower(email));

create table if not exists admin.admin_audit_log (
    id              uuid primary key default gen_random_uuid(),
    admin_user_id   uuid null references admin.admin_users(id) on delete set null,
    admin_email     text not null,
    action          text not null,
    entity_type     text not null,
    entity_id       text null,
    clinic_id       uuid null,
    details         text null,
    created_at      timestamptz not null default now()
);

create index if not exists ix_admin_audit_log_created on admin.admin_audit_log (created_at desc);
create index if not exists ix_admin_audit_log_clinic on admin.admin_audit_log (clinic_id, created_at desc);

-- Supabase exposes the public schema over its REST API; these tables must never be reachable there.
alter table admin.admin_users enable row level security;
alter table admin.admin_audit_log enable row level security;
