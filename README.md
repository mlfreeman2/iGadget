# iGadget

Years ago, Google had a service named iGoogle.
It was a customizable home page that allowed you to add little boxes with various things such as RSS feeds, weather, stocks, etc.
Each one of those was named a "gadget".

I built a nice home page for myself that I liked.

Google, well, they're Google, so they decided to kill it off.

I got annoyed and wrote a version in PHP with all sorts of fancy things like dragging and dropping to move the boxes around (as faithful a recreation of the original as I felt motivated to do).

Over time I've rewritten the front and back ends in various technologies as a learning mechanism and trimmed away functionality I realized I just didn't bother to use.


This latest version is a single ASP.NET Core 3.1 application.
It uses Hangfire to run tasks at periodic intervals to populate the page.
The database backend is SQLite because I didn't want to hassle with an actual database server.
