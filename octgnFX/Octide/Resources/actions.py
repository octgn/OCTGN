import re

###################
#### CONSTANTS ####
###################

## Change these strings to whatever you choose to name your piles from the XML

deck = "Deck"
discard = "Discard"

## Change this HEX string value to customize the highlight color
highlight = "#ff0000"

# Change this positive integer value to customize the default Draw Many count
drawManyDefault = 5

# Change this tuple if you want to create a specific default marker (not recommended)
StandardMarker = ("Marker", "d9851c6f-2ed7-4ca9-82d2-f22e4e12114c")

######################
#### PILE ACTIONS ####
######################

def shuffle(group, x = 0, y = 0):
    mute()
    group.shuffle()
    notify("{} shuffles their {}.".format(me, group.name))

def draw(group, x = 0, y = 0):
    mute()
    if len(group) < 1:
        return
    card = group.top()
    card.moveTo(card.owner.hand)
    notify("{} draws a card from {}.".format(me, group.name))

def drawMany(group, x = 0, y = 0):
    if len(group) < 1:
        return
    mute()
    global drawManyDefault
    count = askInteger("Draw how many cards?", drawManyDefault)
    drawManyDefault = count
    for card in group.top(count):
        card.moveTo(card.owner.hand)
    notify("{} draws {} cards from {}.".format(me, count, group.name))

def discardMany(group, x = 0, y = 0):
    if len(group) < 1:
        return
    mute()
    count = askInteger("Discard how many cards?", 1)
    for card in group.top(count):
        card.moveTo(card.owner.piles[discard])
    notify("{} discards {} cards from {}.".format(me, count, group.name))

def allToDeck(group, x = 0, y = 0):
    mute()
    for card in group:
        card.moveTo(card.owner.piles[deck])
    notify("{} moves all cards from {} to {}.".format(me, group.name, me.piles[deck].name))

######################
#### HAND ACTIONS ####
######################

def randomDiscard(group, x = 0, y = 0):
    mute()
    card = group.random()
    if card == None:
        return
    card.moveTo(me.piles[discard])
    notify("{} randomly discards {} from {}.".format(me, card, group.name))

def randomPick(group, x = 0, y = 0):
    mute()
    card = group.random()
    if card == None:
        return
    if confirm("Reveal randomly-picked {}?".format(card.Name)):
        index = card.index
        card.moveTo(me.piles[discard])
        notify("{} randomly picks {} from their {}.".format(me, card, group.name))
        card.moveTo(me.hand, index)
    else:
        notify("{} randomly picks {} (hidden) from their {}.".format(me, card, group.name))
    card.select()
    card.target(True)

#######################
#### TABLE ACTIONS ####
#######################

def rollDice(group, x = 0, y = 0):
    mute()
    n = rnd(1, 6)
    notify("{} rolls {} on a 6-sided die.".format(me, n))

def flipCoin(group, x = 0, y = 0):
    mute()
    n = rnd(1, 2)
    if n == 1:
        notify("{} flips heads.".format(me))
    else:
        notify("{} flips tails.".format(me))

def interrupt(group, x = 0, y = 0):
    notify('{} interrupts the game.'.format(me))

def passTurn(group, x = 0, y = 0):
    notify('{} passes.'.format(me))

def addAnyMarker(cards, x = 0, y = 0):
    mute()
    marker, quantity = askMarker()
    if quantity == 0: return
    for card in cards:
        card.markers[marker] += quantity
        notify("{} adds {} {} marker(s) to {}.".format(me, quantity, marker[0], card))

def addMarker(cards, x = 0, y = 0):
    mute()
    for card in cards:
        card.markers[StandardMarker] += 1
        notify("{} adds a marker to {}.".format(me, card))

def removeMarker(cards, x = 0, y = 0):
    mute()
    for card in cards:
        if card.markers[StandardMarker] < 1:
            return
        card.markers[StandardMarker] -= 1
        notify("{} removes a marker from {}.".format(me, card))

def rotate(cards, x = 0, y = 0):
  mute()
  for card in cards:
      card.orientation ^= Rot90
      if card.orientation & Rot90 == Rot90:
        notify('{} turns {} sideways'.format(me, card))
      else:
        notify('{} turns {} upright'.format(me, card))

def flip(cards, x = 0, y = 0):
    mute()
    for card in cards:
        if card.isFaceUp == True:
          notify("{} flips {} face down.".format(me, card))
          card.isFaceUp = False
        else:
          card.isFaceUp = True
          notify("{} flips {} face up.".format(me, card))

def highlightCard(cards, x = 0, y = 0):
  mute()
  for card in cards:
      if card.highlight == highlight:
        card.highlight = None
        notify('{} removes highlight from {}'.format(me, card))
      else:
        card.highlight = highlight
        notify('{} highlights {}'.format(me, card))


