from System.IO import Directory, Path
from System.Collections.Generic import *
from System import *
#Rotation constants
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

def wd(relativePath):
	return Path.Combine(_wd,relativePath)

def notify(message):
	_api.Notify(message)

def notifyBar(color, message):
	_api.NotifyBar(color, message)

def whisper(message):
	_api.Whisper(message)

def rnd(min, max):
	return _api.Random(min, max)

def rndArray(min, max, count):
	return _api.RandomArray(min, max, count);

def webRead(url, timeout=0):
	apiResult = _api.Web_Read(url, timeout)
	return (apiResult.Item1, apiResult.Item2)

def webPost(url, data, timeout=0):
	apiResult = _api.Web_Post(url, data, timeout)
	return (apiResult.Item1, apiResult.Item2)

def currentGameName():
	return _api.GetGameName()

def playSound(name):
	return _api.PlaySound(name)

def turnNumber():
	return _api.TurnNumber()

def nextTurn(player = None, force = False):
	if player == None:
		_api.NextTurn(force)
	else:
		_api.NextTurn(player._id, force)

def getActivePlayer():
	id = _api.GetActivePlayer()
	if id == None:
		return None
	return Player(id)

def setActivePlayer(player = None):
	if player == None:
		_api.SetActivePlayer()
	else:
		_api.SetActivePlayer(player._id)

def currentPhase():
	apiResult = _api.GetCurrentPhase()
	return (apiResult.Item1, apiResult.Item2)

def setPhase(id, force = False):
	_api.SetPhase(id, force)

def getStop(id):
	return _api.GetStop(id)

def setStop(id, stop):
	_api.SetStop(id, stop)

def openUrl(url):
	return _api.Open_URL(url)

def confirm(message):
	return _api.Confirm(message)

def askInteger(question, defaultAnswer):
	return _api.AskInteger(question, defaultAnswer)

def askString(question, defaultAnswer):
	return _api.AskString(question, defaultAnswer)

def saveFileDlg(title, defaultPath, fileFilter):
    return _api.SaveFileDlg(title, defaultPath, fileFilter)

def openFileDlg(title, defaultPath, fileFilter):
	return _api.OpenFileDlg(title, defaultPath, fileFilter)

def askChoice(question, choices = [], colors = [], customButtons = []):
	choiceList = List[String](choices)
	if len(colors) != len(choices):
		colors = []
		for count in choices:
			colors.append('None')
	colorList = List[String](colors)
	buttonList = List[String](customButtons)
	apiResult = _api.AskChoice(question, choiceList, colorList, buttonList)
	return apiResult

def askMarker():
	apiResult = _api.AskMarker()
	if apiResult == None: return (None, 0)
	return ((apiResult.Item1, apiResult.Item2), apiResult.Item3)

class cardDlg(object):
	title = "Choose card"
	label = ""
	bottomLabel = ""
	text = None

	def __init__(self, list, bottomList = None):
		self.list = list
		self.bottomList = bottomList
		self._min = None
		self._max = None
	@property
	def min(self): return self._min
	@min.setter
	def min(self, value):
		if value < 0: raise ValueError("Minimum value can't be negative.")
		else: self._min = value
	@property
	def max(self): return self._max
	@max.setter
	def max(self, value):
		if value < 0: raise ValueError("Maximum value can't be negative.")
		else: self._max = value
	def show(self):
		intList = List[int]([c._id for c in self.list])
		if self.bottomList == None:
			intBottomList = None
		else:
			intBottomList = List[int]([c._id for c in self.bottomList])
		apiResult = _api.SelectMultiCard(intList, intBottomList, self._min, self._max, self.text, self.title, self.label, self.bottomLabel)
		if apiResult == None:    ## if the window was closed
			return
		self.list = [Card(c) for c in apiResult.allCards]
		if apiResult.allCards2 != None:
			self.bottomList = [Card(c) for c in apiResult.allCards2]
		self.selected = [Card(c) for c in apiResult.selectedCards]
		return self.selected

def askCard(properties = {}, operator = None, title = "Choose card"):
	realDict = Dictionary[String, List[String]]()
	for (propKey, propValue) in properties.items():
		if type(propValue) is list:
			realDict[propKey] = List[String](propValue)
		else:
			realDict[propKey] = List[String]([propValue])
	apiResult = _api.AskCard(realDict,operator,title)
	if apiResult == None: return (None, 0)
	return (apiResult.Item1, apiResult.Item2)

def queryCard(properties = {}, exact = False):
	realDict = Dictionary[String, List[String]]()
	for (propKey, propValue) in properties.items():
		if type(propValue) is list:
			realDict[propKey] = List[String](propValue)
		else:
			realDict[propKey] = List[String]([propValue])
	apiResult = _api.QueryCard(realDict,exact)
	if apiResult == None: return []
	return [x for x in apiResult]

def focus(cards = None):
	if (cards == None):
		_api.ClearFocus()
	else:
		_api.Focus(List[int](c._id for c in cards))

def clearFocus():
	_api.ClearFocus()

def getFocus():
	ret = _api.GetFocusedCards()
	if ret == None: return None
	return [Card(x) for x in _api.GetFocusedCards()]

def getGlobalVariable(gname):
	return _api.GetGlobalVariable(gname)

def setGlobalVariable(gname,gvalue):
	_api.SetGlobalVariable(gname,gvalue)

def getSetting(name,default):
	return _api.GetSetting(name,default)

def setSetting(name,value):
	_api.SaveSetting(name,value)

def remoteCall(player,func,args):
	realArgs = convertToArgsString(args)
	#notify("Sending remote call {}({}) to {}".format(func,realArgs,player))
	_api.RemoteCall(player._id,func,realArgs)

def choosePack():
	tuple = _api.ChooseCardPackage()
	if tuple == None: return
	return (tuple.Item1, tuple.Item2, tuple.Item3)

def generatePack(model):
	return [x for x in _api.GenerateCardsFromPackage(model)]

def update():
	_api.Update()

def convertToArgsString(obj):
	if type(obj) is list:
		retList = []
		for c in obj:
			retList.append(convertToString(c))
		return ",".join(retList)
	return convertToString(obj)

def convertToString(obj):
	if type(obj) is None:
		return "None"
	if type(obj) is list:
		retList = []
		for c in obj:
			retList.append(convertToString(c))
		return "[" + ",".join(retList) + "]"
	if type(obj) is Player:
		return "Player({})".format(obj._id)
	if isinstance(obj, Group):
		if type(obj) is Table:
			return "table"
		return "Pile({}, '{}', Player({}))".format(obj._id,obj.name.replace("'","\'"),obj.player._id)
	if type(obj) is Card:
		return "Card({})".format(obj._id)
	if type(obj) is Counter:
		return "Counter({},{},{})".format(obj._id,obj.name,obj.player._id)
	if isinstance(obj, basestring):
		return "\"{}\"".format(obj);
	return str(obj)

def showWinForm(form):
	_api.ShowWinForm(form)

def clearSelection(): 
	_api.ClearSelection()

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
	def __setitem__ (self, key, val):
		_api.CardSetProperty(self._id, key, val)
	def __len__(self): return len(cardProperties)
	def __iter__(self): return cardProperties.__iter__()

class Card(object):
	def __init__(self, id):
		self._id = id
		self._props = CardProperties(id)
		self._markers = None
	def __cmp__(self, other):
		if other == None: return 1
		elif not hasattr(other, "_id"): return 1
		return cmp(self._id, other._id)
	def __hash__(self):
		return self._id
	def __getattr__(self, name):
		if name.lower() == "name": return _api.CardName(self._id)
		elif name.lower() in cardProperties: return self.properties[name]
		return object.__getattr__(self, name)
	def __setattr__(self, name, val):
		if name.lower() == "id" or name.lower() == "name": return
		if name.lower() in cardProperties:
			self._props[name] = val
		else:
			super.__setattr__(self, name, val)
	def __format__(self, format_spec):
		return '{#%d}' % self._id
	@property
	def alternate(self): return _api.CardAlternate(self._id)
	@alternate.setter
	def alternate(self, alt = ""):
		_api.CardSwitchTo(self._id,alt)
	@property
	def alternates(self): return _api.CardAlternates(self._id)
	def alternateProperty(self,alt,prop): return _api.CardProperty(self._id,alt,prop)
	def defaultProperty(self, alt, prop): return _api.CardProperty(self._id,alt,prop,True)
	@property
	def model(self): return _api.CardModel(self._id)
	@property
	def name(self): return _api.CardName(self._id)
	@property
	def set(self): return _api.CardSet(self._id)
	@property
	def setId(self): return _api.CardSetId(self._id)
	@property
	def properties(self): return self._props
	def hasProperty(self, prop, alt = None): return _api.CardHasProperty(self._id, prop, alt)
	@property
	def owner(self): return Player(_api.CardOwner(self._id))
	@property
	def controller(self): return Player(_api.CardController(self._id))
	@controller.setter
	def controller(self, player): _api.SetController(self._id, player._id)
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
	def filter(self): return _api.CardGetFilter(self._id)
	@filter.setter
	def filter(self, value): _api.CardSetFilter(self._id, value)
	@property
	def position(self): return _api.CardPosition(self._id)
	@property
	def height(self): return _api.RealHeight(self._id)
	@property
	def width(self): return _api.RealWidth(self._id)
	@property
	def size(self): return _api.CardSize(self._id).Name
	@property
	def markers(self):
		if self._markers == None: self._markers = Markers(self)
		return self._markers
	def moveTo(self, group, index = None):
		_api.CardMoveTo(self._id, group._id, index)
	def moveToBottom(self, pile):
		self.moveTo(pile, len(pile))
	def moveToTable(self, x, y, forceFaceDown = False):
		#notify("{} {}".format(x,y))
		_api.CardMoveToTable(self._id, x, y, forceFaceDown)
	def sendToBack(self):
		_api.CardSetIndex(self._id, 0, True)
	def sendToFront(self):
		_api.CardSetIndex(self._id,len(table), True)
	@property
	def index(self): return _api.CardGetIndex(self._id)
	@index.setter
	def index(self, index):
		_api.CardSetIndex(self._id,index, True)
	@property
	def isSelected(self): return _api.CardIsSelected(self._id)
	def select(self, selection = True): _api.CardSelect(self._id, selection)
	def peek(self): _api.CardPeek(self._id)
	@property
	def peekers(self): return [Player(id) for id in _api.CardPeekers(self._id)]
	def target(self, active = True): _api.CardTarget(self._id, active)
	def arrow(self, targetCard, active = True): _api.CardTargetArrow(self._id, targetCard._id, active)
	@property
	def targetedBy(self):
		playerId = _api.CardTargeted(self._id)
		return Player(playerId) if playerId >= 0 else None
	@property
	def anchor(self):
		return _api.CardAnchored(self._id)
	@anchor.setter
	def anchor(self, anchored):
		_api.CardSetAnchored(self._id,anchored)
	def delete(self):
		_api.CardDelete(self._id)
	def isInverted(self, y = None):
		if y == None: y = self.position[1]
		if not Table.isTwoSided(): return False
		return y < -( self.height / 2 )
	def offset(self, x = None, y = None):
		if x == None: x = self.position[0]
		if y == None: y = self.position[1]
		delta = ( min(self.height, self.width) / 5 ) * ( 1 if not self.isInverted(y) else -1 )
		return (x + delta, y + delta)
	def resetProperties(self):
		_api.CardResetProperties(self._id)

class NamedObject(object):
	def __init__(self, id, name):
		self._id = id
		self._name = name
	def __cmp__(self, other):
		if other == None: return 1
		elif not hasattr(other, "_id"): return 1
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
	@property
	def visibility(self): return _api.GroupGetVisibility(self._id)
	@visibility.setter
	def visibility(self, value): _api.GroupSetVisibility(self._id, value)
	@property
	def viewers(self): return [Player(id) for id in _api.GroupViewers(self._id)]
	def addViewer(self, player): _api.GroupAddViewer(self._id, player._id)
	def removeViewer(self, player): _api.GroupRemoveViewer(self._id, player._id)
	@property
	def controller(self):
		return Player(_api.GroupController(self._id))
	@controller.setter
	def controller(self, player):
		_api.GroupSetController(self._id, player._id)
	def create(self, model, quantity = 1):
		ids = _api.Create(model, self._id, quantity)
		if quantity != 1:
			return [Card(id) for id in ids]
		else:
			return Card(ids[0]) if len(ids) == 1 else None

class Table(Group):
	def __init__(self):
		Group.__init__(self, 0x01000000, 'Table')
	def create(self, model, x, y, quantity = 1, persist = False, facedown = False):
		ids = _api.CreateOnTable(model, x, y, persist, quantity,facedown)
		#if ids == None or ids == []: return None
		if quantity != 1:
			return [Card(id) for id in ids]
		else:
			return Card(ids[0]) if len(ids) == 1 else None
	@property
	def board(self): return _api.GetBoard()
	@board.setter
	def board(self, key): _api.SetBoard(key)
	@property
	def boards(self): return _api.GetBoardList()
	_twoSided = None
	@staticmethod
	def isTwoSided():
		if Table._twoSided == None: Table._twoSided = _api.IsTwoSided()
		return Table._twoSided
	@property
	def invertBackground(self):
		return _api.IsTableBackgroundFlipped()
	@invertBackground.setter
	def invertBackground(self, value):
		_api.SetTableBackgroundFlipped(value)
	def reset(self):
		_api.TableResetScreen()
	def refit(self):
		_api.TableRefitScreen()


class Pile(Group):
	def __init__(self, id, name, player):
		Group.__init__(self, id, name, player)
	def top(self, count = None): return self[0] if count == None else self[:count]
	def bottom(self, count = None): return self[-1] if count == None else self[-count:]
	def shuffle(self): _api.GroupShuffle(self._id)
	@property
	def collapsed(self): return _api.GroupGetCollapsed(self._id)
	@collapsed.setter
	def collapsed(self, value): _api.GroupSetCollapsed(self._id, value)
	@property
	def viewState(self): return _api.PileGetViewState(self._id)
	@viewState.setter
	def viewState(self, value): _api.PileSetViewState(self._id, value)
	def lookAt(self, value, istop = True): _api.GroupLookAt(self._id, value, istop)

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
		self._piles = idict((pair.Value, Pile(pair.Key, pair.Value, self)) for pair in _api.PlayerPiles(id))
	def __cmp__(self, other):
		if other == None: return 1
		elif not hasattr(other, "_id"): return 1
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
	def isActive(self):
		return self._id == _api.GetActivePlayer()
	def setActive(self, force = False): _api.NextTurn(self._id, force)
	@property
	def isSubscriber(self): return _api.IsSubscriber(self._id)
	@property
	def name(self): return _api.PlayerName(self._id)
	@property
	def counters(self): return self._counters
	@property
	def piles(self): return self._piles
	@property
	def color(self): return _api.PlayerColor(self._id)
	@color.setter
	def color(self, colorHex): _api.SetPlayerColor(self._id, colorHex)
	@property
	def isInverted(self): return _api.PlayerHasInvertedTable(self._id)
	def getGlobalVariable(self,gname):
		return _api.PlayerGetGlobalVariable(self._id,gname)
	def setGlobalVariable(self,gname,gvalue):
		_api.PlayerSetGlobalVariable(self._id,gname,gvalue)

class EventArgument(object):
	def __init__(self, dic):
		self._args = dic
	def __getattr__(self, name):
		if name in self._args:
			return self._args[name]
		return None

_id = _api.LocalPlayerId()
me = Player(_id) if _id >= 0 else None
_id = _api.SharedPlayerId()
shared = Player(_id) if _id >= 0 else None
del _id
players = [Player(id) for id in _api.AllPlayers()]

def getPlayers():
	return [Player(id) for id in _api.AllPlayers()]

table = Table()
cardProperties = [x.lower() for x in _api.CardProperties()]
version = _api.OCTGN_Version()
gameVersion = _api.GameDef_Version()
def fd():
	_api.ForceDisconnect()
def resetGame():
	_api.ResetGame()
def softResetGame():
	_api.SoftResetGame()
