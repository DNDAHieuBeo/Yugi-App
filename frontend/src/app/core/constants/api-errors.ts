export const API_ERROR_MESSAGES: Record<string, string> = {
  // Auth
  'Email already in use.': 'This email address is already registered.',
  'Invalid credentials.': 'Incorrect email or password.',
  'Invalid or expired refresh token.': 'Your session has expired. Please sign in again.',
  'User not found.': 'Account not found.',

  // ASP.NET Identity — password rules
  'Passwords must have at least one non alphanumeric character.': 'Password must contain at least one special character.',
  "Passwords must have at least one digit ('0'-'9').": 'Password must contain at least one digit.',
  "Passwords must have at least one uppercase ('A'-'Z').": 'Password must contain at least one uppercase letter.',
  "Passwords must have at least one lowercase ('a'-'z').": 'Password must contain at least one lowercase letter.',
  'Passwords must be at least 6 characters.': 'Password must be at least 6 characters.',

  // Generic server
  'An unexpected error occurred': 'Something went wrong. Please try again later.',
  'Unauthorized': 'You are not authorized to perform this action.',
};

export const DEFAULT_ERROR = 'An error occurred. Please try again.';
