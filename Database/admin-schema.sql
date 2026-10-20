-- =====================================================================
-- Omni Admin Portal — the portal's own table: the admin audit log (schema admin).
--
-- It lives in the SAME database as the main app but is owned by this repo. Admin accounts are not here: since Omni
-- Milestone A they are the main app's users (identity.users with role admin), signed in through the main app's
-- /api/platform-admin/auth API and managed through /api/platform-admin/admins. A client or member account can never sign
-- in to the portal, and an admin account can never sign in to the client app.
--
-- Run it AFTER the main app's Database/schema.sql (the audit log references identity.users).
-- A new project filled by the main app's data copy (Database/migrations/2026-10-generic/copy-from-old.sql) already has this
-- table in this exact shape; running this file there changes nothing. Keep the two definitions identical.
--
-- Idempotent: safe to run more than once, on a fresh database or an older one. Each block checks what is there first.
-- =====================================================================

create schema if not exists admin;

do $$
begin
    if to_regclass('identity.users') is null then
        raise exception 'identity.users is missing: run the main app''s Database/schema.sql on this database first.';
    end if;
end $$;

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

-- 2026-10-20 - admins are main-app users: the audit log's admin_user_id (uuid -> admin.admin_users) becomes user_id
-- (text -> identity.users), then admin.admin_users is dropped. Refuses while any admin is not yet in identity.users with
-- the same id as text (the data copy moves them), so no admin account is lost. No-op on a fresh or already-moved database.
do $$
declare
    missing text;
begin
    if to_regclass('admin.admin_users') is not null then
        select string_agg(a.email, ', ') into missing from admin.admin_users a
        where not exists (select 1 from identity.users u where u.id = a.id::text and u.role = 'admin');
        if missing is not null then
            raise exception 'These admins are not in identity.users (role admin) yet, so admin.admin_users was kept: %. Move them first (the main app''s copy-from-old.sql does).', missing;
        end if;
    end if;

    if exists (select 1 from information_schema.columns where table_schema = 'admin' and table_name = 'admin_audit_log' and column_name = 'admin_user_id') then
        alter table admin.admin_audit_log add column if not exists user_id text null;
        update admin.admin_audit_log l set user_id = l.admin_user_id::text
        where l.admin_user_id is not null and exists (select 1 from identity.users u where u.id = l.admin_user_id::text);
        alter table admin.admin_audit_log drop column admin_user_id; -- drops its foreign key to admin.admin_users too
    end if;

    drop table if exists admin.admin_users;
end $$;

-- The final shape (the same as copy-from-old.sql's).
create table if not exists admin.admin_audit_log (
    id              uuid primary key default gen_random_uuid(),
    user_id         text null references identity.users(id) on delete set null,
    admin_email     text not null,
    action          text not null,
    entity_type     text not null,
    entity_id       text null,
    client_id       uuid null,
    details         text null,
    created_at      timestamptz not null default now()
);

-- A table moved by the block above gets the same foreign key the create gives a new one.
do $$
begin
    if not exists (select 1 from pg_constraint where conrelid = 'admin.admin_audit_log'::regclass and contype = 'f') then
        alter table admin.admin_audit_log
            add constraint admin_audit_log_user_id_fkey foreign key (user_id) references identity.users(id) on delete set null;
    end if;
end $$;

create index if not exists ix_admin_audit_log_created on admin.admin_audit_log (created_at desc);
create index if not exists ix_admin_audit_log_client on admin.admin_audit_log (client_id, created_at desc);

-- Supabase exposes the public schema over its REST API; this table must never be reachable there.
alter table admin.admin_audit_log enable row level security;
