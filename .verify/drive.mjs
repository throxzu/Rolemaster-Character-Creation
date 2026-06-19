import { chromium } from 'playwright';

const BASE = 'http://localhost:5224';
const out = (m) => console.log(m);

const browser = await chromium.launch();
const ctx = await browser.newContext({ ignoreHTTPSErrors: true, viewport: { width: 1400, height: 1000 } });
const page = await ctx.newPage();

try {
  // 1. Login
  await page.goto(`${BASE}/account/login`, { waitUntil: 'networkidle' });
  await page.fill('input[autocomplete="username"]', 'gamemaster@rolemaster.local');
  await page.fill('input[autocomplete="current-password"]', 'GM@rolemaster1');
  await Promise.all([
    page.waitForLoadState('networkidle'),
    page.click('button:has-text("Log in")'),
  ]);
  out('LOGIN_URL_AFTER=' + page.url());

  // 2. Reference Tables page
  await page.goto(`${BASE}/reference-tables`, { waitUntil: 'networkidle' });
  await page.waitForTimeout(1500); // let Blazor interactive circuit connect
  out('REF_URL=' + page.url());
  out('PAGE_TITLE=' + await page.title());

  // List items
  const items = await page.$$eval('.rt-list-item', els => els.map(e => e.textContent.trim().replace(/\s+/g,' ')));
  out('LIST_ITEMS=' + JSON.stringify(items));
  const groups = await page.$$eval('.rt-group-header', els => els.map(e => e.textContent.trim()));
  out('GROUPS=' + JSON.stringify(groups));

  // Default selected table title + notes + grid header
  out('SELECTED_TITLE=' + (await page.locator('.rt-title').first().textContent().catch(()=>'(none)')));
  out('SELECTED_META=' + (await page.locator('.rt-meta').first().textContent().catch(()=>'(none)')));
  await page.screenshot({ path: 'shot-default.png', fullPage: true });

  // 3. Click each pilot table and capture its grid
  for (const name of ['Stat Gains', 'Resistance Rolls']) {
    await page.click(`.rt-list-item:has-text("${name}")`);
    await page.waitForTimeout(400);
    const title = await page.locator('.rt-title').first().textContent();
    const cols = await page.$$eval('.rt-grid thead th', els => els.map(e=>e.textContent.trim()));
    const rowCount = await page.$$eval('.rt-grid tbody tr', els => els.length);
    const firstRow = await page.$$eval('.rt-grid tbody tr:first-child td', els => els.map(e=>e.textContent.trim()));
    out(`TABLE[${name}] title=${title} cols=${JSON.stringify(cols)} rows=${rowCount} firstRow=${JSON.stringify(firstRow)}`);
    out(`NOTES[${name}]=` + (await page.locator('.rt-notes').first().textContent().catch(()=>'(none)')).slice(0,120));
  }
  await page.screenshot({ path: 'shot-resistance.png', fullPage: true });

  // 4. Probe: search filter
  await page.fill('input[placeholder="Search tables…"]', 'gain');
  await page.waitForTimeout(400);
  const afterSearch = await page.$$eval('.rt-list-item', els => els.map(e => e.textContent.trim().replace(/\s+/g,' ')));
  out('SEARCH[gain]_ITEMS=' + JSON.stringify(afterSearch));

  // 5. Probe: search with no matches
  await page.fill('input[placeholder="Search tables…"]', 'zzzznotable');
  await page.waitForTimeout(400);
  const noMatch = await page.locator('.rt-list .text-muted').first().textContent().catch(()=>'(none)');
  out('SEARCH[zzzz]_EMPTYMSG=' + noMatch);

  out('RESULT=OK');
} catch (e) {
  out('RESULT=ERROR ' + e.message);
  await page.screenshot({ path: 'shot-error.png', fullPage: true }).catch(()=>{});
} finally {
  await browser.close();
}
