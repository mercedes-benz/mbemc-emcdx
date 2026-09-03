# Commit Message Guidelines

All commit messages MUST follow this format and MUST
be written in English.

## Structure

1. **Summary line**: A short summary of the change,
   maximum 50 characters.
2. **Blank line**: Exactly one empty line after the
   summary.
3. **Body**: 1 to 8 bullet points, each line with a
   maximum of 50 characters, describing what was
   changed.

## Bullet Point Prefixes

Each bullet point must start with one of these
prefixes:

- `B` — Bugfix
- `+` — Something was added
- `-` — Something was removed

## Sorting

Bullet points MUST be sorted by prefix in this order:

1. `B` (bugfixes) first
2. `+` (additions) second
3. `-` (removals) last

## Language

- All commit messages MUST be written in English.

## Example
Add percentual generator mode

B Fix rounding error in power calculation
+ Add percentage-based generator setpoint
+ Add unit tests for generator mode
- Removed obsolete fixed-value fallback
