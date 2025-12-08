# Issue Tracker - Demo User Accounts

## Pre-seeded Test Accounts

The application comes with the following pre-seeded accounts for testing different roles:

### Admin Account
- **Email:** admin@issue.local
- **Password:** Admin123!
- **Roles:** Admin + Engineer
- **Permissions:**
  - Full access to all features
  - Can manage user roles
  - Can view/edit/delete all issues
  - Can assign/unassign issues
  - Can comment on all issues

### Engineer Account
- **Email:** engineer@issue.local
- **Password:** Engineer123!
- **Role:** Engineer
- **Permissions:**
  - Can view all issues
  - Can assign issues to themselves
  - Can update issue status
  - Can comment on all issues
  - Cannot manage user roles

### Customer Account
- **Email:** customer@issue.local
- **Password:** Customer123!
- **Role:** Customer
- **Permissions:**
  - Can create new issues
  - Can view only their own issues
  - Can edit their own issues (title, description, priority)
  - Can comment only on their own issues
  - Cannot change issue status
  - Cannot assign issues

## Registration Flow

New users can register and select their role:
- **Customer:** For users who need to report and track issues
- **Engineer:** For technical staff who resolve issues

Admin role can only be assigned by existing admins through the User Management interface.

## Testing the System

1. **As a Customer:**
   - Sign in with customer@issue.local
   - Create new issues
   - View "My Issues"
   - Comment on your own issues
   - Edit issue details (but not status)

2. **As an Engineer:**
   - Sign in with engineer@issue.local
   - View "All Issues"
   - View "Assigned to Me"
   - Assign issues to yourself
   - Update issue status
   - Comment on any issue

3. **As an Admin:**
   - Sign in with admin@issue.local
   - Access "User Management" to change user roles
   - Full access to all Engineer capabilities
   - Can delete issues
   - Can unassign issues from other engineers

## Navigation Features

The navigation menu dynamically changes based on user role:
- **All Users:** Dashboard, My Issues
- **Engineers/Admins:** All Issues, Assigned to Me
- **Admins Only:** User Management

## Key Features Implemented

? Role-based registration
? Role-based navigation
? Issue assignment system
? Comments system
? Dashboard with statistics
? Responsive design
? Security permissions
? Landing page for anonymous users
? User management interface