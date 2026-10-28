# Features found in old releases

Code that shipped to kiosks in some release, was switched off or never used, and was later removed. Found by
rebuilding the archived update-store snapshots ([snapshots.md](snapshots.md)); the decompiled source of every
release is on the `history/*` branches ([version-history.md](version-history.md)).

Most switches are server configuration, so the code shows only the compiled-in default. What was actually
switched on is judged from that default, from how deeply the feature is wired into the UI, from the URLs it
uses, and, for 2024, from the kiosk image's own engine configuration (`REDS\Kiosk Engine\data\system.config`).
The Kiosk Client Service configuration, which holds the marketing switches, is not in the image. Where the
evidence is indirect, it says so.

| Feature | Releases | Switch (default) | Verdict |
|---|---|---|---|
| [Event tickets](#event-tickets-2014) | Braveheart, Dracula (2014) | `General/SellTickets` + a ticket catalogue | installed, never had anything to sell |
| [Starz offer](#starz-subscription-offer-20222023) | LincolnSR3 1.71 – Z 1.85 | `MarketingGeneral/EnableStarzOffer` (off) | probably live for a while |
| [Fraud barcode tracking](#fraud-services-barcode-tracking-and-disc-images-20192023) | KISS1.1 1.44 – Z 1.85 | `Fraud/MonitorBarcodeTracking` (off) | off on the 2024 kiosk; gutted in 1.86 |
| [Welcome screen](#welcome-screen-ab-test-and-assets-20192023) | KISS1.1 1.44 – Willys 1.82 | A/B value; KCS `EnableAssetsSync` (off) | test feature, replaced by Campaign |
| [E-mail opt-in campaign](#e-mail-opt-in-campaign-20192023) | PrimusSR4 1.49 – YugoSR2 1.84 | `General/ShowOptinForNormalCheckout` (on) | live where content shipped a campaign |
| [Dual kiosk](#dual-kiosk-20232024) | Yugo 1.84 – Calvin 1.88 | KCS `Browse.ShowDualInStock` (off) | complete, use unknown |

Not in the table, because they shipped and stayed: **Campaign** (Tesla 1.79 on: start-screen carousel and
in-cart promotions from the Kiosk Client Service, `CMS.ShowStartScreen` / `CMS.EnableInCart` on by default),
**Redbox.FMA.UI** (LincolnSR3 1.71 on: the technicians' field-maintenance planogram screens, frozen at 1.72.0.5),
and **reservations** (reserve online, pick up at the kiosk) which run through every release since KISS1.1.

## Event tickets (2014)

**Where:** engine Braveheart (1.3.7.10, 2014-02-03) and Dracula (1.3.9.4, 2014-02-28):
`Redbox.DVD.TicketServices.dll` (kernel extension `ticket-services.kext`) in `kop-v2-engine`, and the Lua library
`library-tickets.bundle` ("Library Tickets" 1.1.0.2) in `kop-rental-bundles`. Branch `history/engine`, project
`src/KioskEngine/Redbox.DVD.TicketServices`; branch `history/rental-bundles`, folder `bundles/library-tickets`.

**What it is:** selling tickets to live events (concerts, sports, theatre) at the kiosk, with seat selection,
paid by card and delivered by e-mail. The Lua scripts are dated 2012.

- **Data:** the kiosk keeps a local catalogue in a VistaDB database `tickets-profile.data` plus
  `tickets-images.zip`, staged as `staged_tickets-*` and activated together. The model has productions, events,
  venues (address, parking, ADA accessibility), price codes, ticket groups, a "Top 80" event list, browse and
  carousel lists, and on-sale/off-sale times per production. The data is refreshed every 120 s.
- **Server:** a WCF service, `IControllerService` (`ControllerService.svc`), with `GetTickets` (reserve seats:
  returns ticket groups and a lock GUID), `UpdateTicketLock` / `ReleaseTicketLock`, `PlaceOrder` (customer,
  order header, items, price codes, taxes, payment, promotions), `GetEmailByCard` and `SendNotification`
  (e-mail subscriptions). The URL comes from the `system/General/TicketServicesURL` configuration value; the
  compiled-in default is a development host (`tickets-dev.redbox.com`).
- **Engine integration:** Lua kernel functions `kernel.ticketsvcs_*` (`gettickets`, `placeorder`,
  `updateticketlock`, `releaseticketlock`, `getemailbycard`, `sendnotification`, `selltickets`,
  `issellticketson`, `datatablegetall`, `getbrowseevents`, `getcarouselevents`, `refreshifneeded`,
  `refreshviews`, `excludeproduction`, `registerimagefromcache`) and five preference pages under "Remote
  Services\Ticket Services" (General, Email Messages, Locking, Place Order, Get Tickets). The preference pages
  can simulate the server (busy, timeout, communication failure, chosen response codes, canned ticket groups)
  for testing.
- **UI flow** (`library-tickets.bundle`, 49 Lua scripts and their views): find tickets / browse (carousel,
  paging), production selection, venue details, seating chart and seat selection, choose quantity, promotional
  rules and supplier disclaimer, shopping cart, delivery method, customer name, e-mail, ZIP code, card swipe,
  authorisation, purchase confirmation, "how it works".
- **Switch:** `kernel.ticketsvcs_selltickets` is true only if `system/General/SellTickets` is true, an activated
  ticket catalogue exists, and its Top 80 list is not empty. The 2014 rental application (`ra-2.x.bundle`) loads
  the ticket libraries at start-up and has a tickets button on its start screen (`abe_tickets_button`).

**Deployment:** no revision log in any of the 24 snapshots lists a ticket catalogue (`tickets-profile.data`);
the only data files ever shipped through the store are `dvd-data`'s `profile.data` and `inventory.data`. So in
these snapshots the code was installed but had nothing to sell. Whether some kiosks received a catalogue by other
means (for example a pilot) cannot be told from the snapshots.

**Removal:** the store never lists a file as removed, so the Dracula builds stay in the revision logs until
2024. Kiosks lost them through the engine's activation script instead: `afteractivate.lua` (header comment
"06/4/2014"; first recoverable in the 2015-12 snapshot) runs `kernel.filedelete` on
`kext\Redbox.DVD.TicketServices.dll`, `kext\ticket-services.kext` and `bundles\library-tickets.bundle`, and
every later engine release keeps those lines. The `history/*` branches apply such deletions, so the ticket code
leaves them at that release. The rental application kept ticket references in its e-mail and
wait views until 2019 and a `image-tickets-red-background` resource until the end (2024).

## Starz subscription offer (2022–2023)

**Where:** LincolnSR3 (1.71.0.21) to Z (1.85.0.27); removed in Aquaman2023 (1.86). Branch `history/engine`,
`src/Rental/Redbox.Rental.Services/Starz`.

**What it is:** a STARZ subscription sold at the kiosk: a 7-day free trial plus two one-night disc rental codes a
month. It appears as an offer popup in browse (`BrowseViewController`) and in the shopping cart
(`ShoppingCartViewModel.StarzOfferVisibility`), with an offer-detail view, terms acceptance and an e-mail
keyboard (`StarzOfferDetailView`, `StarzTermsAcceptanceView`, `KeyboardStarzSignupView`). Sign-up goes through
`StarzSignupService`, which checks the customer's subscriptions (`HasActiveStarzSubscription`) through the Kiosk
Client Service. The products are `StarzSubscriptionProduct` offers 1–4 with an `IsFreeTrial` flag.

**Switch:** `marketing/MarketingGeneral/EnableStarzOffer`, default `false`. The cart offer is also hidden in ADA
mode, for ineligible customers, and with more than two items in the cart. The Preference Manager can inject test
subscription plans through the Kiosk Client Service's test data.

**Deployment:** nothing turns it on in code. But the offer's QR codes point at production pages
(`redbox.com/starz/special-offer?icamp=...starzspecialoffer...`) and it is wired into the main browse and cart
flow, so it was most likely switched on from the server for a while (inferred). After removal, 1.88 still has the
`starz-*` images, `AccountSourceType.Starz` and `HasActiveStarzSubscription`.

## Fraud services: barcode tracking and disc images (2019–2023)

**Where:** KISS1.1 (1.44.1.3) to Z (1.85); `Redbox.FraudServices.Proxy` and `FraudServices` folders in
`Redbox.KioskEngine.ComponentModel` / `.Environment`. Aquaman2023 (1.86) deleted about 1,500 lines of it.

**What it is:** every disc read by the HAL camera (`CameraStatsJson`) becomes a `BarcodeTrackingInfo` record
(barcodes and secure codes read, camera generation, decoder, `MarkedAsFraud`), batched through the message
queue to `api/Fraud/addbarcodetracking`; images of returned discs go to `api/Fraud/discimage`.

**Switches (`system/Fraud`):** `MonitorBarcodeTracking` (default `false`), `FraudServicesUrl` (later also
`AwsFraudServicesUrl`, default empty), `EnableIRSecurity` and `EnableImageSecurity` (default `false`),
`SendReturnImageTypes` (default games). The disc-image uploader is always started, but with no URL it sends
nothing.

**Deployment:** the 2024 kiosk's configuration has `MonitorBarcodeTracking = False`. No hard-coded host. In 1.88
only the HAL fraud-disc read (`kernel.hardwarereadfrauddisc`, reported to the Kiosk Client Service's
`api/inventory/discfraud`) and the old-image cleanup remain; `BarcodeTrackingService` only logs.

## Welcome screen: A/B test and assets (2019–2023)

**A/B test (KISS1.1 1.44 to McLaren/Nova 1.73):** the start screen's screensaver shows hard-coded "kiosk screen
test" images (`2475.1_KioskScreenTest_PRM-Screen`, `2475.4_App_KioskScreenTest_App`, ...) that lead to For Sale
browse and a "reserve online" popup, only when the A/B value `WelcomeScreenABTestVersion` is B or D. The default
is A (off).

**Welcome Screen Assets (Pinto 1.75 to Willys 1.82; gone in Xterra 1.83):** `StartViewController` fetches
images from the Kiosk Client Service (`GET api/welcomescreenassets`) and shows them when the idle timer fires;
tapping one opens that title. The Kiosk Client Service side (`WelcomeScreenAssetsController`, a job every 10
minutes downloading from pre-signed URLs) is gated by `MarketingWelcomeScreen.EnableAssetsSync`, default
`false`. Vega also carries hard-coded June 2022 artwork (Morbius, Ambulance, The Batman). Replaced by Campaign
(Tesla 1.79), which does the same through the CMS and is on by default.

## E-mail opt-in campaign (2019–2023)

**Where:** PrimusSR4 (1.49.0.110) to YugoSR2 (1.84.1.14); Z removed the service, the model classes are dead
code in 1.88.

**What it is:** an e-mail opt-in screen in checkout (`EmailOptInViewController`) with a campaign id, template,
image and program name taken from an `opt_in_campaign` resource in the content bundle (Lua
`kernel.load_email_optin_campaign`).

**Switch:** shown when the customer gave an e-mail, text-to-speech is off, the campaign resource exists and
`system/General/ShowOptinForNormalCheckout` is true (default true; true on the 2024 kiosk). So it was live
wherever the content carried a campaign; whether it did was not checked.

## Dual kiosk (2023–2024)

**Where:** Yugo (1.84.0.25) to Calvin (1.88, current); Kiosk Client Service from its Yugo release.

**What it is:** support for two kiosks at one store. Browse and title details can say "in stock at kiosk N" /
"at the other kiosk", with a learn-more popup (`dual_in_stock_learn_more_popup_view`). The Kiosk Client Service
(`DualService`; `api/dual/refresh`, `api/dual/updatetitles`, `api/dual/instockinother`) asks the proxy for the
other kiosk's title counts; the other kiosk is the nearby kiosk with `IsDual`.

**Switch:** Kiosk Client Service configuration `Browse.ShowDualInStock`, default `false`, plus a dual kiosk
must exist. The code is complete and still in 1.88; whether any store enabled it cannot be told from the
snapshots or the image.
