# Pile Protection Feature

The Pile Protection feature allows game designers and players to control who can view the contents of piles (hands, decks, discard piles, etc.). This feature adds security to competitive play by preventing unauthorized viewing of private piles.

## Overview

The pile protection system offers three protection states:

- **`false`** - Allow viewing (default, maintains current behavior)
- **`true`** - Block viewing completely  
- **`ask`** - Request permission to view (posts chat message)

## Game Definition Support

Game designers can set default protection levels in their game definition XML files:

```xml
<pile name="Hand" visibility="owner" protectionState="ask" />
<pile name="Deck" visibility="me" protectionState="true" />
<pile name="Discard" visibility="all" protectionState="false" />
```

### XML Schema

The `protectionState` attribute can be added to any `<pile>` element:

- **Attribute:** `protectionState`
- **Type:** String enumeration
- **Values:** `"false"`, `"true"`, `"ask"`
- **Default:** `"false"`
- **Optional:** Yes

## User Interface

### Context Menu Integration

Right-click on any pile to access the "Protection" submenu with three checkable options:

- **Allow viewing** - Sets protection to `false`
- **Block viewing** - Sets protection to `true`  
- **Ask permission** - Sets protection to `ask`

The menu shows the current protection state with a checkmark next to the active option.

### Protection Behavior

- **Allow viewing (`false`)**: No restrictions, pile can be viewed normally
- **Block viewing (`true`)**: All viewing attempts are blocked with a message
- **Ask permission (`ask`)**: Viewing is blocked, but a chat message is posted requesting permission

### Owner Bypass

Pile owners can always view their own piles regardless of protection state.

## Scripting API

Two new scripting functions are available across all script versions (3.1.0.0, 3.1.0.1, 3.1.0.2):

### PileGetProtectionState(id)

Gets the current protection state of a pile.

**Parameters:**
- `id` (int): The pile ID

**Returns:**
- String: `"false"`, `"true"`, or `"ask"`
- `null`: If the ID doesn't correspond to a pile

**Example:**
```python
# Check protection state of player's hand
hand_state = myt.PileGetProtectionState(me.piles['Hand'].Id)
if hand_state == "true":
    notify("Hand is protected from viewing")
```

### PileSetProtectionState(id, state)

Sets the protection state of a pile.

**Parameters:**
- `id` (int): The pile ID
- `state` (string): Protection state - `"false"`, `"true"`, or `"ask"`

**Returns:**
- None

**Example:**
```python
# Protect all player decks at game start
def onGameStart():
    for player in getPlayers():
        deck = player.piles['Deck']
        myt.PileSetProtectionState(deck.Id, "true")
        
    # Set hands to require permission
    myt.PileSetProtectionState(me.piles['Hand'].Id, "ask")
```

## Protected Actions

The pile protection system protects against:

- Right-click "Look at" menu options
- Scripted `GroupLookAt()` calls
- All other pile viewing mechanisms

## Chat Messages

When someone attempts to view a pile with `"ask"` protection, a chat message is automatically posted:

```
[Player Name] requests permission to view [Pile Name]
```

The pile owner can then change the protection state to allow viewing if desired.

## Examples

### Competitive Deck Protection

```python
def onGameStart():
    # Completely protect all player decks
    for player in getPlayers():
        if 'Deck' in player.piles:
            myt.PileSetProtectionState(player.piles['Deck'].Id, "true")
```

### Permission-Based Hand Viewing

```python
def onGameStart():
    # Set all hands to require permission
    for player in getPlayers():
        if 'Hand' in player.piles:
            myt.PileSetProtectionState(player.piles['Hand'].Id, "ask")
```

### Dynamic Protection Based on Game State

```python
def onTurnStart():
    # Only protect hands during opponent's turn
    current_player = getCurrentPlayer()
    for player in getPlayers():
        if player != current_player and 'Hand' in player.piles:
            myt.PileSetProtectionState(player.piles['Hand'].Id, "true")
        elif 'Hand' in player.piles:
            myt.PileSetProtectionState(player.piles['Hand'].Id, "false")
```

## Backwards Compatibility

The pile protection feature is fully backwards compatible:

- Existing games continue to work unchanged (default state is `"false"`)
- Old game definitions without `protectionState` attributes work normally
- No breaking changes to existing APIs or behaviors

## Technical Implementation

- **Core Enum:** `GroupProtectionState` with values `False`, `True`, `Ask`
- **Data Model:** Added `ProtectionState` property to `Group` class
- **Serialization:** XML serialization support for game definitions
- **UI Integration:** Context menu additions to `GroupControl` and `PileBaseControl`
- **Script API:** Functions added to all script versions for compatibility
- **Protection Enforcement:** Checks added to all pile viewing entry points