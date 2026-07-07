# HEVEQ Final Patch Notes v7

## Frontend

- Employee field visits page now uploads evidence photos as files through `POST /api/media/images` instead of asking the employee to paste image links.
- Home navbar guest actions now show both `إنشاء حساب جديد` and `تسجيل الدخول`.
- Login and Register password fields now have show/hide password toggles.
- Admin users page now supports search by name, email, or phone number.
- Admin users page pagination design was improved through the shared pagination component.
- Admin users page now includes UI actions to create a new Admin or Employee.
- Employee creation supports department selection, governorate assignment, and dispatch availability.
- Added admin review moderation page at `/admin/reviews` for AI-flagged reviews.
- Added sidebar link for `مراجعة التقييمات` under admin navigation.
- Removed the admin dashboard urgent/pending tasks section from `/admin`.

## Backend

- Added `POST /api/AdminUsers/admins` to let Admin create another Admin account.
- Reused existing `POST /api/admin/employees` endpoint for creating Employee accounts from the admin users page.
- Added `GET /api/admin/reviews/pending` to retrieve AI-flagged or pending reviews with pagination.
- Added `POST /api/admin/reviews/{id}/approve` to publish a pending review and refresh provider rating aggregates when relevant.
- Added `POST /api/admin/reviews/{id}/reject` to reject a pending review.
- Translated key employee/admin creation errors to Arabic.

## Verification

Frontend build was verified with:

```bash
cd Frontend
npm run build
```

Backend build must be verified locally because this environment does not have the .NET SDK:

```bash
cd Backend
dotnet build HEVEQ.Api/HEVEQ.Api.csproj
dotnet run --project HEVEQ.Api
```

No new database migration was added in this patch.
