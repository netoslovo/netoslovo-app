insert into auth.roles
    (id, name, normalized_name, concurrency_stamp)
values
    (
        '11111111-1111-1111-1111-111111111111',
        'Admin',
        'ADMIN',
        '22222222-2222-2222-2222-222222222222'
    )
on conflict (normalized_name)
where normalized_name is not null
do nothing;