import { Temporal } from 'temporal-polyfill';
import { describe, expect, it } from 'vitest';

import { ContentType, contentTypeCatalog, emptyContent } from '@/domain/codes';

// Payload encoding lives on the backend; the builder's content already is the wire shape.
describe('emptyContent', () => {
  it('creates minimal typed content with the discriminator set and required fields blank', () => {
    expect(emptyContent(ContentType.Url)).toEqual({ type: 'url', url: '' });
    expect(emptyContent(ContentType.Wifi)).toEqual({ type: 'wifi', ssid: '', encryption: 'wpa', hidden: false });
    expect(emptyContent(ContentType.MobileApp)).toEqual({ type: 'mobileApp', store: 'appStore', url: '' });
    expect(emptyContent(ContentType.Geo)).toEqual({ type: 'geo', latitude: 0, longitude: 0 });
  });

  it('seeds a calendar event with a whole-minute start', () => {
    const content = emptyContent(ContentType.Calendar);
    if (content.type !== ContentType.Calendar) throw new Error('Expected calendar content.');
    expect(content.start).toBeInstanceOf(Temporal.PlainDateTime);
    expect(content.start.second).toBe(0);
  });

  it('covers every catalogued type exactly once', () => {
    const ids = contentTypeCatalog.map((entry) => entry.id);
    expect(new Set(ids).size).toBe(ids.length);
    for (const id of ids) expect(emptyContent(id).type).toBe(id);
  });
});
