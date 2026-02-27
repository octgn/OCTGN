import { describe, it, expect } from 'vitest';
import { parseDefinitionXml } from '@main/games/definition-parser';

describe('parseDefinitionXml — table, board, and card size parsing', () => {
  const minimalGame = (inner: string) => `<?xml version="1.0" encoding="utf-8"?>
<game id="abc-123" name="Test Game" version="1.0.0" gameversion="0.0.0.1">
  <card name="Default" width="63" height="88" cornerRadius="5" back="cards/back.png" front="cards/front.png"
        backWidth="63" backHeight="88" backCornerRadius="5" />
  <player name="Player">
    <group name="Hand" visibility="owner" ordered="false" />
  </player>
  ${inner}
</game>`;

  describe('table parsing', () => {
    it('parses table with just width and height', () => {
      const xml = minimalGame(`<table name="Table" width="640" height="480" />`);
      const def = parseDefinitionXml(xml);
      expect(def).not.toBeNull();
      expect(def!.table).toBeDefined();
      expect(def!.table!.name).toBe('Table');
      expect(def!.table!.width).toBe(640);
      expect(def!.table!.height).toBe(480);
      expect(def!.table!.background).toBeUndefined();
      expect(def!.table!.backgroundStyle).toBeUndefined();
    });

    it('parses table with background and backgroundStyle', () => {
      const xml = minimalGame(
        `<table name="Table" width="800" height="600" background="images/bg.png" backgroundStyle="tile" />`
      );
      const def = parseDefinitionXml(xml);
      expect(def!.table!.background).toBe('images/bg.png');
      expect(def!.table!.backgroundStyle).toBe('tile');
    });

    it('parses all backgroundStyle values', () => {
      for (const style of ['stretch', 'tile', 'uniform', 'uniformToFill'] as const) {
        const xml = minimalGame(
          `<table name="T" width="100" height="100" backgroundStyle="${style}" />`
        );
        const def = parseDefinitionXml(xml);
        expect(def!.table!.backgroundStyle).toBe(style);
      }
    });

    it('handles missing table element gracefully', () => {
      const xml = minimalGame('');
      const def = parseDefinitionXml(xml);
      expect(def).not.toBeNull();
      expect(def!.table).toBeUndefined();
    });

    it('defaults table dimensions to 0 when attributes are missing', () => {
      const xml = minimalGame(`<table name="T" />`);
      const def = parseDefinitionXml(xml);
      expect(def!.table!.width).toBe(0);
      expect(def!.table!.height).toBe(0);
    });
  });

  describe('board parsing — legacy boardPosition on table', () => {
    it('parses board from table board + boardPosition attributes', () => {
      const xml = minimalGame(
        `<table name="Table" width="640" height="480" board="images/board.png" boardPosition="10,20,300,200" />`
      );
      const def = parseDefinitionXml(xml);
      expect(def!.table!.board).toBeDefined();
      expect(def!.table!.board!.name).toBe('Default');
      expect(def!.table!.board!.source).toBe('images/board.png');
      expect(def!.table!.board!.x).toBe(10);
      expect(def!.table!.board!.y).toBe(20);
      expect(def!.table!.board!.width).toBe(300);
      expect(def!.table!.board!.height).toBe(200);
    });

    it('does not set board when boardPosition is missing', () => {
      const xml = minimalGame(
        `<table name="Table" width="640" height="480" board="images/board.png" />`
      );
      const def = parseDefinitionXml(xml);
      expect(def!.table!.board).toBeUndefined();
    });
  });

  describe('gameboards parsing', () => {
    it('parses a default gameboard from gameboards element', () => {
      const xml = minimalGame(
        `<table name="Table" width="640" height="480" />
         <gameboards name="Main Board" src="boards/main.png" x="0" y="0" width="500" height="400" />`
      );
      const def = parseDefinitionXml(xml);
      expect(def!.boards).toBeDefined();
      expect(def!.boards!.length).toBeGreaterThanOrEqual(1);
      expect(def!.boards![0]).toEqual({
        name: 'Main Board',
        source: 'boards/main.png',
        x: 0,
        y: 0,
        width: 500,
        height: 400,
      });
    });

    it('parses additional gameboard children', () => {
      const xml = minimalGame(
        `<table name="Table" width="640" height="480" />
         <gameboards name="Default" src="boards/default.png" x="0" y="0" width="500" height="400">
           <gameboard name="Alt Board" src="boards/alt.png" x="10" y="20" width="300" height="250" />
           <gameboard name="Night" src="boards/night.png" x="5" y="5" width="500" height="400" />
         </gameboards>`
      );
      const def = parseDefinitionXml(xml);
      expect(def!.boards).toBeDefined();
      // Default + 2 children
      expect(def!.boards!.length).toBe(3);
      expect(def!.boards![0].name).toBe('Default');
      expect(def!.boards![1].name).toBe('Alt Board');
      expect(def!.boards![1].source).toBe('boards/alt.png');
      expect(def!.boards![2].name).toBe('Night');
    });

    it('returns empty boards array when no gameboards element exists and no legacy board', () => {
      const xml = minimalGame(`<table name="Table" width="640" height="480" />`);
      const def = parseDefinitionXml(xml);
      // No gameboards element and no legacy board → no boards
      expect(def!.boards).toBeUndefined();
    });
  });

  describe('card size parsing', () => {
    it('parses default card size from card element', () => {
      const xml = `<?xml version="1.0" encoding="utf-8"?>
<game id="abc-123" name="Test Game" version="1.0.0">
  <card name="Default" width="63" height="88" cornerRadius="5"
        back="cards/back.png" front="cards/front.png"
        backWidth="65" backHeight="90" backCornerRadius="6" />
  <table name="T" width="100" height="100" />
  <player name="Player">
    <group name="Hand" visibility="owner" ordered="false" />
  </player>
</game>`;
      const def = parseDefinitionXml(xml);
      expect(def!.defaultCardSize).toBeDefined();
      expect(def!.defaultCardSize!.name).toBe('Default');
      expect(def!.defaultCardSize!.width).toBe(63);
      expect(def!.defaultCardSize!.height).toBe(88);
      expect(def!.defaultCardSize!.cornerRadius).toBe(5);
      expect(def!.defaultCardSize!.back).toBe('cards/back.png');
      expect(def!.defaultCardSize!.front).toBe('cards/front.png');
      expect(def!.defaultCardSize!.backWidth).toBe(65);
      expect(def!.defaultCardSize!.backHeight).toBe(90);
      expect(def!.defaultCardSize!.backCornerRadius).toBe(6);
    });

    it('defaults backWidth/backHeight/backCornerRadius to front values when negative', () => {
      const xml = `<?xml version="1.0" encoding="utf-8"?>
<game id="abc-123" name="Test Game" version="1.0.0">
  <card name="Default" width="63" height="88" cornerRadius="5"
        back="back.png" front="front.png"
        backWidth="-1" backHeight="-1" backCornerRadius="-1" />
  <table name="T" width="100" height="100" />
  <player name="Player">
    <group name="Hand" visibility="owner" ordered="false" />
  </player>
</game>`;
      const def = parseDefinitionXml(xml);
      expect(def!.defaultCardSize!.backWidth).toBe(63);
      expect(def!.defaultCardSize!.backHeight).toBe(88);
      expect(def!.defaultCardSize!.backCornerRadius).toBe(5);
    });

    it('defaults backWidth/backHeight/backCornerRadius to front values when missing', () => {
      const xml = `<?xml version="1.0" encoding="utf-8"?>
<game id="abc-123" name="Test Game" version="1.0.0">
  <card name="Default" width="63" height="88" cornerRadius="5"
        back="back.png" front="front.png" />
  <table name="T" width="100" height="100" />
  <player name="Player">
    <group name="Hand" visibility="owner" ordered="false" />
  </player>
</game>`;
      const def = parseDefinitionXml(xml);
      expect(def!.defaultCardSize!.backWidth).toBe(63);
      expect(def!.defaultCardSize!.backHeight).toBe(88);
      expect(def!.defaultCardSize!.backCornerRadius).toBe(5);
    });

    it('parses alternative card sizes from card > size children', () => {
      const xml = `<?xml version="1.0" encoding="utf-8"?>
<game id="abc-123" name="Test Game" version="1.0.0">
  <card name="Default" width="63" height="88" cornerRadius="5"
        back="back.png" front="front.png"
        backWidth="63" backHeight="88" backCornerRadius="5">
    <size name="Small" width="40" height="56" cornerRadius="3"
          back="small/back.png" front="small/front.png"
          backWidth="40" backHeight="56" backCornerRadius="3" />
    <size name="Large" width="90" height="126" cornerRadius="8"
          back="large/back.png" front="large/front.png"
          backWidth="-1" backHeight="-1" backCornerRadius="-1" />
  </card>
  <table name="T" width="100" height="100" />
  <player name="Player">
    <group name="Hand" visibility="owner" ordered="false" />
  </player>
</game>`;
      const def = parseDefinitionXml(xml);
      expect(def!.cardSizes).toBeDefined();
      expect(Object.keys(def!.cardSizes!)).toHaveLength(2);
      expect(def!.cardSizes!['Small']).toBeDefined();
      expect(def!.cardSizes!['Small'].width).toBe(40);
      expect(def!.cardSizes!['Small'].height).toBe(56);
      expect(def!.cardSizes!['Large']).toBeDefined();
      // negative back dimensions should fall back to front
      expect(def!.cardSizes!['Large'].backWidth).toBe(90);
      expect(def!.cardSizes!['Large'].backHeight).toBe(126);
      expect(def!.cardSizes!['Large'].backCornerRadius).toBe(8);
    });

    it('handles card element with no size children', () => {
      const xml = minimalGame(`<table name="T" width="100" height="100" />`);
      const def = parseDefinitionXml(xml);
      expect(def!.defaultCardSize).toBeDefined();
      expect(def!.cardSizes).toEqual({});
    });
  });

  describe('backward compatibility', () => {
    it('still populates legacy cardWidth, cardHeight, cardBack fields', () => {
      const xml = `<?xml version="1.0" encoding="utf-8"?>
<game id="abc-123" name="Test Game" version="1.0.0">
  <card name="Default" width="63" height="88" cornerRadius="5"
        back="cards/back.png" front="cards/front.png"
        backWidth="63" backHeight="88" backCornerRadius="5" />
  <table name="T" width="100" height="100" />
  <player name="Player">
    <group name="Hand" visibility="owner" ordered="false" />
  </player>
</game>`;
      const def = parseDefinitionXml(xml);
      expect(def!.cardWidth).toBe(63);
      expect(def!.cardHeight).toBe(88);
      expect(def!.cardBack).toBe('cards/back.png');
    });
  });
});
