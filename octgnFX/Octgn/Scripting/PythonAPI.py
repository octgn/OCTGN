﻿#Rotation constants
Rot0 = 0
Rot90 = 1
Rot180 = 2
Rot270 = 3

def mute():
  class Muted(object):
    def __enter__(self):
      return self
    def __exit__(self, type, value, tb):
      _api.Mute(False)
  _api.Mute(True)
  return Muted()

def notify(message):
  _api.Notify(message)

def whisper(message):
  _api.Whisper(message)

def rnd(min, max):
  return _api.Random(min, max)

def webRead(url):
  apiResult = _api.Web_Read(url)
  return (apiResult.Item1, apiResult.Item2)

def openUrl(url):
  return _api.Open_URL(url)

def confirm(message):
  return _api.Confirm(message)

def askInteger(question, defaultAnswer):
  return _api.AskInteger(question, defaultAnswer)

def askMarker():
  apiResult = _api.AskMarker()
  if apiResult == None: return (None, 0)
  return ((apiResult.Item1, apiResult.Item2), apiResult.Item3)

def askCard(restriction = None):
  apiResult = _api.AskCard(restriction)
  if apiResult == None: return (None, 0)
  return (apiResult.Item1, apiResult.Item2)

def getGlobalVariable(gname):
  return _api.GetGlobalVariable(gname)

def setGlobalVariable(gname,gvalue): 
  _api.SetGlobalVariable(gname,gvalue)

class Markers(object):
  def __init__(self, card):
    self._cardId = card._id
  def __getitem__(self, key):
    return _api.MarkerGetCount(self._cardId, *key)
  def __setitem__(self, key, value):
    _api.MarkerSetCount(self._cardId, value, *key)
  def __delitem__(self, key):
    _api.MarkerSetCount(self._cardId, 0, *key)
  def __len__(self):
    return len(_api.CardGetMarkers(self._cardId))
  def __iter__(self):
    return ((m.Item1, m.Item2) for m in _api.CardGetMarkers(self._cardId))
  def __contains__(self, key):
    return _api.MarkerGetCount(self._cardId, *key) > 0

class CardProperties(object):
  def __init__(self, cardId):
    self._id = cardId
  def __getitem__(self, key):
    return _api.CardProperty(self._id, key)
  def __len__(self): return len(cardProperties)
  def __iter__(self): return cardProperties.__iter__()

class Card(object):
  def __init__(self, id):
    self._id = id
    self._props = CardProperties(id)    
    self._markers = None
  def __cmp__(self, other):
    if other == None: return 1
    return cmp(self._id, other._id)
  def __hash__(self):
    return self._id
  def __getattr__(self, name):
    if name.lower() in cardProperties:
      return self.properties[name]
    return object.__getattr__(self, name)
  def __format__(self, format_spec):
    return '{#%d}' % self._id
  @property
  def isAlternateImage(self): return _api.IsAlternateImage(self._id)
  @property
  def switchImage(self): _api.SwitchImage(self._id)
  @property
  def isAlternate(self): return _api.IsAlternate(self._id)
  @property
  def switchWithAlternate(self): _api.SwitchWithAlternate(self._id)
  @property
  def model(self): return _api.CardModel(self._id)
  @property
  def name(self): return _api.CardName(self._id)
  @property
  def properties(self): return self._props
  @property
  def owner(self): return Player(_api.CardOwner(self._id))
  @property
  def controller(self): return Player(_api.CardController(self._id))
  @property
  def group(self): return eval(_api.GroupCtor(_api.CardGroup(self._id)))
  @property
  def isFaceUp(self): return _api.CardGetFaceUp(self._id)
  @isFaceUp.setter
  def isFaceUp(self, value): _api.CardSetFaceUp(self._id, value)
  @property
  def orientation(self): return _api.CardGetOrientation(self._id)
  @orientation.setter
  def orientation(self, value): _api.CardSetOrientation(self._id, value)
  @property
  def highlight(self): return _api.CardGetHighlight(self._id)
  @highlight.setter
  def highlight(self, value): _api.CardSetHighlight(self._id, value)
  @property
  def position(self): return _api.CardPosition(self._id)
  @property
  def markers(self):
    if self._markers == None: self._markers = Markers(self)
    return self._markers
  def moveTo(self, group, index = None):
    _api.CardMoveTo(self._id, group._id, index)
  def moveToBottom(self, pile):
    self.moveTo(pile, len(pile))
  def moveToTable(self, x, y, forceFaceDown = False):
    _api.CardMoveToTable(self._id, x, y, forceFaceDown)
  def sendToBack(self):
    _api.CardSetIndex(self._id, 0, True)
  def sendToFront(self):
    _api.CardSetIndex(self._id,len(table), True)
  def setIndex(self, index):
    _api.CardSetIndex(self._id,index, True)
  @property
  def getIndex(self): return _api.CardGetIndex(self._id)
  def select(self): _api.CardSelect(self._id)
  def target(self, active = True): _api.CardTarget(self._id, active)
  @property
  def targetedBy(self):
    playerId = _api.CardTargeted(self._id)
    return Player(playerId) if playerId >= 0 else None
  _width = None
  _height = None
  @staticmethod
  def _fetchSize():
    size = _api.CardSize()
    Card._width = size.Item1
    Card._height = size.Item2
  @staticmethod
  def width():
    if Card._width == None: Card._fetchSize()
    return Card._width
  @staticmethod
  def height():
    if Card._height == None: Card._fetchSize()
    return Card._height

class NamedObject(object):
  def __init__(self, id, name):
    self._id = id
    self._name = name
  def __cmp__(self, other):
    if other == None: return 1
    return cmp(self._id, other._id)
  def __hash__(self):
    return self._id
  @property
  def name(self): return self._name

class Group(NamedObject):
  def __init__(self, id, name, player = None):
    NamedObject.__init__(self, id, name)
    self._player = player
  @property
  def player(self): return self._player
  def __len__(self): return _api.GroupCount(self._id)
  def __getitem__(self, key):
    if key < 0 : key += len(self)
    return Card(_api.GroupCard(self._id, key))
  def __iter__(self): return (Card(id) for id in _api.GroupCards(self._id))
  def __getslice__(self, i, j): return (Card(id) for id in _api.GroupCards(self._id)[i:j])
  def random(self):
    count = len(self)
    if count == 0: return None
    return self[rnd(0, count - 1)]

class Table(Group):
  def __init__(self):
    Group.__init__(self, 0x01000000, 'Table')
  def create(self, model, x, y, quantity = 1, persist = False):
    ids = _api.CreateOnTable(model, x, y, persist, quantity)
    #if ids == None or ids == []: return None
    if quantity != 1:
      return [Card(id) for id in ids]
    else:
      return Card(ids[0]) if len(ids) == 1 else None    
  _twoSided = None
  @staticmethod
  def isTwoSided():
    if Table._twoSided == None: Table._twoSided = _api.IsTwoSided()
    return Table._twoSided
  @staticmethod
  def isInverted(y):
    if not Table.isTwoSided(): return False
    return y < Card.height() / 2
  _offset = None
  @staticmethod
  def offset(x, y):
    if Table._offset == None: Table._offset = min(Card.width(), Card.height()) / 5
    delta = Table._offset if not Table.isInverted(y) else -Table._offset
    return (x + delta, y + delta)

class Hand(Group):
  def __init__(self, id, player):
    Group.__init__(self, id, 'Hand', player)

class Pile(Group):
  def __init__(self, id, name, player):
    Group.__init__(self, id, name, player)
  def top(self, count = None): return self[0] if count == None else self[:count]
  def bottom(self, count = None): return self[-1] if count == None else self[-count:]
  def shuffle(self): _api.GroupShuffle(self._id)

class Counter(NamedObject):
  def __init__(self, id, name, player):
    NamedObject.__init__(self, id, name)
    self._player = player
  @property
  def player(self): return self._player
  @property
  def value(self): return _api.CounterGet(self._id)
  @value.setter
  def value(self, value): _api.CounterSet(self._id, value)

class Player(object):
  def __init__(self, id):
    self._id = id
    self._counters = idict((pair.Value, Counter(pair.Key, pair.Value, self)) for pair in _api.PlayerCounters(id))
    handId = _api.PlayerHandId(id)

    self._hand = Hand(handId, self) if handId != 0 else None
    self._piles = idict((pair.Value, Pile(pair.Key, pair.Value, self)) for pair in _api.PlayerPiles(id))
  def __cmp__(self, other):
    return cmp(self._id, other._id)
  def __hash__(self):
    return self._id
  def __getattr__(self, name):
    if name in self._piles: return self._piles[name]
    if name in self._counters: return self._counters[name].value
    return object.__getattr__(self, name)
  def __setattr__(self, name, value):
    if not name.startswith("_"):
      if name in self._piles: raise AttributeError("You can't overwrite a Player's pile.")
      if name in self._counters:
        self._counters[name].value = value
        return
    object.__setattr__(self, name, value)
  def __format__(self, format_spec): return self.name
  @property
  def isActivePlayer(self): return _api.IsActivePlayer(self._id)
  @property
  def name(self): return _api.PlayerName(self._id)
  @property
  def counters(self): return self._counters
  @property
  def hand(self): return self._hand
  @property
  def piles(self): return self._piles
  @property
  def color(self): return _api.PlayerColor(self._id)
  def hasInvertedTable(self): return _api.PlayerHasInvertedTable(self._id)
  def getGlobalVariable(self,gname):
    return _api.PlayerGetGlobalVariable(self._id,gname)
  def setGlobalVariable(self,gname,gvalue): 
    _api.PlayerSetGlobalVariable(self._id,gname,gvalue)

_id = _api.LocalPlayerId()
me = Player(_id) if _id >= 0 else None
_id = _api.SharedPlayerId()
shared = Player(_id) if _id >= 0 else None
del _id
players = [Player(id) for id in _api.AllPlayers()]
table = Table()
cardProperties = [x.lower() for x in _api.CardProperties()]
version = _api.OCTGN_Version()
gameVersion = _api.GameDef_Version()