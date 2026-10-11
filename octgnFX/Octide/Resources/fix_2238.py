import re

def parse_resource_uses(text):
    """
    Parse text like "uses (X)" to extract the number of resource tokens.
    Returns the number found, or 0 if no digits are found.
    
    This fixes the issue where missing else clause caused crashes when
    no digits were found in "uses (...)" text.
    """
    if not text:
        return 0
    
    # Look for digits inside "uses (...)" pattern
    match = re.search(r'uses\s*\(\s*(\d+)\s*\)', text, re.IGNORECASE)
    if match:
        return int(match.group(1))
    else:
        # No digits found in uses pattern - return 0 instead of crashing
        return 0