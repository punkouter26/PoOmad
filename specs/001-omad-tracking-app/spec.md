# Feature Specification: PoOmad - Minimalist OMAD Tracker

**Feature Branch**: `001-omad-tracking-app`  
**Created**: 2025-11-22  
**Status**: Draft  
**Input**: User description: "PoOmad: The Minimalist Accountability Partner for Intermittent Fasting. OMAD (One Meal A Day) tracking with binary philosophy - you either stuck to the plan or didn't. Features: friction-free setup, dark mode calendar dashboard, visual status indicators, 10-second daily logging, smart analytics, streak tracking."

## Clarifications

### Session 2025-11-22

- Q: User Account Model - Single user per device vs multi-user with authentication vs cloud-synced? → A: Google OAuth authentication for multi-user support with unique data per user
- Q: Data Retention & Privacy - Auto-delete after inactivity vs indefinite retention vs export-only after 90 days? → A: Retain data indefinitely with user-initiated deletion option for long-term analytics
- Q: Weight Change Validation Threshold - What weight change triggers confirmation prompt? → A: Allow any changes up to 5 pounds daily variation without confirmation; changes exceeding 5 lbs require verification
- Q: Offline Sync Conflict Resolution - Client-side merge vs last write wins vs device priority? → A: Last write wins based on server timestamp (most recent edit overwrites)
- Q: Calendar Start Day of Week - Sunday vs Monday vs user-configurable? → A: Sunday (traditional US calendar format)

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Initial Setup & Profile Creation (Priority: P1)

A new user opens PoOmad for the first time and is prompted to sign in with Google OAuth. After successful authentication, they are guided through a streamlined onboarding process to establish their baseline metrics. The system captures height and starting weight to create a personalized profile linked to their Google account. This initial setup is completed in under 2 minutes. Multiple users can use the app, each with their own unique data isolated by account.

**Why this priority**: Without a user profile, no tracking can occur. This is the entry point to the entire application and must be completed before any other functionality is available. It establishes the baseline for weight tracking and ensures data isolation between users.

**Independent Test**: Can be fully tested by launching the app as a new user, authenticating with Google, entering height and weight, and verifying the profile is created and the user is redirected to the calendar dashboard. Logging out and signing in with a different Google account should show a separate, empty profile. Delivers immediate value by establishing the user's starting point with secure data isolation.

**Acceptance Scenarios**:

1. **Given** a user launches PoOmad for the first time, **When** the app opens, **Then** a Google OAuth sign-in prompt appears
2. **Given** the Google sign-in prompt is displayed, **When** the user successfully authenticates with their Google account, **Then** the setup wizard appears with fields for height and starting weight
3. **Given** the setup wizard is displayed, **When** the user enters valid height (e.g., 5'10") and weight (e.g., 180 lbs), **Then** the profile is created and linked to their Google account, and the user is navigated to the calendar dashboard
4. **Given** the user enters invalid data (e.g., negative weight), **When** they attempt to proceed, **Then** validation errors are displayed clearly
5. **Given** the user completes setup, **When** they reopen the app and are already authenticated, **Then** they are taken directly to the calendar dashboard, not the setup wizard
6. **Given** a user is authenticated, **When** they sign out and a different user signs in with a different Google account, **Then** that second user sees their own isolated data (or setup wizard if new)

---

### User Story 2 - Daily OMAD Logging (Priority: P1)

A user clicks on "Today" in the calendar dashboard and is presented with a streamlined modal asking three simple questions: Did you do OMAD? (Yes/No), Did you drink Alcohol? (Yes/No), and What is your current weight? The entire interaction takes less than 10 seconds and provides immediate visual feedback on the calendar.

**Why this priority**: This is the core interaction of the entire application - the daily accountability check-in. Without this, the app has no purpose. It must be frictionless and fast to encourage daily adherence.

**Independent Test**: Can be fully tested by selecting the current day on the calendar, completing the three-question form, submitting, and verifying the calendar updates with a green (success) or red (missed) indicator. Delivers immediate value by creating the first visual "chain" of consistency.

**Acceptance Scenarios**:

1. **Given** the user is on the calendar dashboard, **When** they click on today's date, **Then** a modal appears with three fields: OMAD (Yes/No toggle), Alcohol (Yes/No toggle), Current Weight (number input)
2. **Given** the logging modal is open, **When** the user selects "Yes" for OMAD, "No" for Alcohol, enters current weight, and submits, **Then** today's calendar cell displays a green indicator and the modal closes
3. **Given** the logging modal is open, **When** the user selects "No" for OMAD and submits, **Then** today's calendar cell displays a red indicator
4. **Given** the user has already logged today, **When** they click on today's date, **Then** the modal pre-fills with their existing data and allows editing
5. **Given** the user enters a weight change exceeding 5 pounds from their last entry, **When** they submit, **Then** a confirmation prompt asks them to verify the entry before saving

---

### User Story 3 - Calendar Dashboard & Visual Consistency Chain (Priority: P2)

The user views their monthly journey as a grid-based calendar where each week starts on Sunday (traditional US format) and each day displays a visual status indicator. Successful OMAD days appear as vibrant green cells, missed days as muted red cells, and unlogged days remain neutral. The dashboard provides at-a-glance feedback on monthly adherence and displays the current streak count prominently.

**Why this priority**: The calendar visualization is the primary motivational mechanism. It transforms abstract data into a visual "chain" that users don't want to break. This is essential for long-term engagement but can be delivered after basic logging exists.

**Independent Test**: Can be fully tested by logging several days (mix of success/failure), navigating between months, and verifying that visual indicators display correctly, weeks start on Sunday, and the streak count updates accurately. Delivers value by providing motivational feedback and pattern recognition.

**Acceptance Scenarios**:

1. **Given** the user has logged multiple days, **When** they view the calendar dashboard, **Then** green cells appear for successful OMAD days and red cells for missed days, with weeks starting on Sunday
2. **Given** the user is viewing the calendar, **When** they navigate to previous or next months, **Then** the calendar updates to show that month's data with consistent Sunday start
3. **Given** the user has a streak of consecutive OMAD days, **When** they view the dashboard, **Then** the "Current Streak" counter displays the correct number of consecutive green days
4. **Given** the user logs a missed day, **When** the calendar updates, **Then** the streak counter resets to 0
5. **Given** a day has not been logged, **When** viewing the calendar, **Then** that day appears in a neutral state (not green or red)

---

### User Story 4 - Weight & Alcohol Analytics (Priority: P3)

The user navigates to a statistics section and views a sophisticated combo chart that overlays their weight trend line with vertical bars representing alcohol consumption days. The chart intelligently handles missing data by carrying forward previous weight entries to maintain smooth trend lines. Users can visually identify correlations between alcohol consumption and weight fluctuations.

**Why this priority**: Analytics provide insights and long-term value, but are not essential for the core tracking functionality. Users need several weeks of data before patterns emerge, making this lower priority for initial MVP.

**Independent Test**: Can be fully tested by logging weight and alcohol data over multiple days, navigating to the analytics view, and verifying that the combo chart displays correctly with weight trend lines and alcohol bars overlaid. Delivers value by revealing behavioral patterns that impact weight loss.

**Acceptance Scenarios**:

1. **Given** the user has logged at least 7 days of data, **When** they navigate to the analytics section, **Then** a combo chart displays with weight as a line graph and alcohol consumption as vertical bars
2. **Given** the user has gaps in their logging history, **When** the chart is rendered, **Then** the weight trend line intelligently carries forward the last known weight to prevent breaks in the visualization
3. **Given** the chart is displayed, **When** the user hovers over a data point, **Then** a tooltip shows the specific weight and whether alcohol was consumed that day
4. **Given** the user has less than 3 days of data, **When** they navigate to analytics, **Then** a message displays: "Log at least 3 days to see your trends"
5. **Given** the user views the chart, **When** they identify a pattern (e.g., alcohol consumption correlating with weight increases), **Then** the visual overlay makes this correlation immediately obvious

---

### User Story 5 - Dark Mode Interface (Priority: P3)

The entire application interface is rendered in a professional dark mode theme designed to reduce eye strain and focus attention on the data. All screens, modals, and charts use a consistent dark palette with high-contrast text and visual indicators.

**Why this priority**: While dark mode enhances the user experience and aligns with the "professional" branding, it is a polish feature that doesn't impact core functionality. Can be implemented later as a refinement.

**Independent Test**: Can be fully tested by navigating through all screens (setup, dashboard, logging modal, analytics) and verifying that all UI elements use the dark mode color scheme with appropriate contrast ratios for accessibility. Delivers value by reducing eye strain for daily use.

**Acceptance Scenarios**:

1. **Given** the user opens any screen in PoOmad, **When** the interface loads, **Then** all backgrounds use dark colors, text uses high-contrast light colors, and interactive elements are clearly visible
2. **Given** the user is viewing the calendar dashboard, **When** they observe the visual indicators, **Then** green and red cells maintain vibrant contrast against the dark background
3. **Given** the user opens the logging modal, **When** the form displays, **Then** input fields and toggles are styled for dark mode with clear focus states
4. **Given** the user views the analytics chart, **When** the chart renders, **Then** gridlines, axes, and data visualizations are optimized for dark backgrounds

---

### Edge Cases

- What happens when a user tries to log a date in the future? (System should prevent or warn)
- What happens when a user's weight entry is zero or negative? (Validation should reject)
- What happens when a user skips multiple days and then logs inconsistently? (Chart should handle gaps gracefully; unlogged days don't break streak, only logged non-compliant days do)
- What happens when a user tries to navigate to a month before their start date? (Show empty calendar or disable navigation)
- What happens when a user changes their weight by more than 5 pounds from their last entry? (Confirmation prompt to verify data accuracy per FR-017b)
- What happens when a user has no internet connection? (Data cached locally from cloud, synced when online - see FR-019)
- What happens when Google OAuth authentication fails or is cancelled? (User remains on authentication screen with clear error message)
- What happens when a user's authentication session expires while using the app? (Gracefully prompt re-authentication without losing unsaved data)
- What happens when the same Google account is used on multiple devices simultaneously? (Cloud sync ensures consistent data across devices)
- What happens when the same day is edited on multiple devices while offline? (Last write wins based on server timestamp per FR-019a; most recent edit is kept when devices sync)
- What happens when a user deletes their account? (All profile data and log entries are permanently removed; action requires confirmation)
- What happens to data when a user becomes inactive for months or years? (Data is retained indefinitely per FR-022; no automatic deletion)

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST authenticate users via Google OAuth before granting access to the application
- **FR-002**: System MUST isolate user data by authenticated account, ensuring each user only accesses their own profile and log entries
- **FR-003**: System MUST capture and persist user profile data including height and starting weight during initial setup, linked to the authenticated Google account
- **FR-004**: System MUST validate height input to accept common formats (feet/inches, centimeters) and reasonable ranges (4' to 8' or 120cm to 250cm)
- **FR-005**: System MUST validate weight input to accept reasonable ranges (50 lbs to 500 lbs or 25 kg to 250 kg)
- **FR-006**: System MUST display a monthly calendar grid with visual status indicators for each day, starting each week on Sunday
- **FR-007**: System MUST allow users to log daily data consisting of three fields: OMAD compliance (boolean), alcohol consumption (boolean), and current weight (decimal number)
- **FR-008**: System MUST persist daily log entries with date, OMAD status, alcohol status, and weight, scoped to the authenticated user
- **FR-009**: System MUST calculate and display the current streak of consecutive OMAD-compliant days for the authenticated user
- **FR-010**: System MUST reset the streak counter to zero when a non-compliant day is logged
- **FR-011**: System MUST allow users to edit previously logged entries belonging to their account
- **FR-012**: System MUST generate a combo chart visualizing weight trend (line) and alcohol consumption (bars) over time for the authenticated user
- **FR-013**: System MUST handle missing data points in the weight trend chart by carrying forward the last known weight value to maintain visual continuity
- **FR-014**: System MUST apply a dark mode color scheme to all user interface elements
- **FR-015**: System MUST prevent users from logging dates in the future
- **FR-016**: System MUST navigate between months in the calendar view (previous/next month)
- **FR-017**: System MUST validate weight entries and display appropriate errors/confirmations:
  - **FR-017a**: Allow weight entries that vary by 5 pounds or less from the previous entry without additional confirmation
  - **FR-017b**: Display a confirmation prompt when weight entries exceed 5 pounds variation from the previous entry, requiring user verification before saving
  - **FR-017c**: Reject invalid data (negative weight, non-numeric input, values outside 50-500 lbs range per FR-005)
- **FR-018**: System MUST distinguish between unlogged days (neutral state) and logged days (green/red state) in the calendar
- **FR-019**: System MUST support cloud data storage with offline caching to enable cross-device access and offline functionality
- **FR-019a**: System MUST resolve sync conflicts using last-write-wins strategy based on server timestamp when the same day is edited on multiple devices while offline
- **FR-020**: System MUST provide tooltip/detail view when interacting with chart data points
- **FR-021**: System MUST provide a sign-out function that clears the current session and returns to the authentication screen
- **FR-022**: System MUST retain all user data indefinitely to support long-term trend analysis
- **FR-023**: System MUST provide a user-initiated account deletion function that permanently removes all associated profile data and log entries

### Key Entities

- **User Profile**: Represents the user's baseline metrics linked to their Google account. Attributes: Google account ID (unique identifier), email (from Google OAuth), height, starting weight, start date (date of first setup completion).
- **Daily Log Entry**: Represents a single day's accountability check-in for a specific user. Attributes: user ID (foreign key to User Profile), date, OMAD compliance (boolean), alcohol consumed (boolean), current weight (decimal), server timestamp (used for last-write-wins conflict resolution during offline sync).
- **Streak**: Calculated value representing consecutive days of OMAD compliance for a specific user. Not necessarily stored, but derived from daily log entries scoped to the user.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: New users can complete initial setup and create their profile in under 2 minutes
- **SC-002**: Users can log their daily OMAD status in under 10 seconds from opening the app
- **SC-003**: The calendar dashboard loads and displays the current month within 1 second on standard devices
- **SC-004**: Users can visually identify their adherence pattern at a glance without reading numbers or text (via green/red color coding)
- **SC-005**: The streak counter accurately reflects consecutive OMAD days with zero calculation errors
- **SC-006**: The analytics chart displays complete weight trends even when 20% of days have missing weight data (via intelligent gap-filling)
- **SC-007**: 90% of users successfully log their first day without requiring help documentation or tooltips
- **SC-008**: The application functions offline, with data persisting locally and syncing when connectivity is restored
