/** Card is in the inverted zone when its Y is above the midline by more than half its height */
export function isInInvertedZone(y: number, cardHeight: number): boolean {
  return y < -cardHeight / 2;
}
