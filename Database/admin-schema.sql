-- =====================================================================
-- Omni Admin Portal — admin-only tables.
--
-- These live in the SAME database as the main app (Supabase Postgres) but are owned by this repo.
-- The main app never reads them, and client member accounts (identity_users) can never sign in to the
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
    client_id       uuid null,
    details         text null,
    created_at      timestamptz not null default now()
);

create index if not exists ix_admin_audit_log_created on admin.admin_audit_log (created_at desc);

-- Supabase exposes the public schema over its REST API; these tables must never be reachable there.
alter table admin.admin_users enable row level security;
alter table admin.admin_audit_log enable row level security;

-- 2026-10-19 - Omni rename: the audit log's old client column and index get their new names (no-op on a fresh database)
do $$
begin
    if exists (select 1 from information_schema.columns where table_schema = 'admin' and table_name = 'admin_audit_log' and column_name = 'clinic_id') then -- leftover-ok: migrates the old column name
        alter table admin.admin_audit_log rename column clinic_id to client_id; -- leftover-ok: migrates the old column name
    end if;
    if exists (select 1 from pg_indexes where schemaname = 'admin' and indexname = 'ix_admin_audit_log_clinic') then -- leftover-ok: migrates the old index name
        alter index admin.ix_admin_audit_log_clinic rename to ix_admin_audit_log_client; -- leftover-ok: migrates the old index name
    end if;
end $$;
create index if not exists ix_admin_audit_log_client on admin.admin_audit_log (client_id, created_at desc);
