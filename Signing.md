# Introduction #

Magellan's SNK is included in the solution. This surprised a few people.

The only reason Magellan is signed to begin with is for people who want to use it in situations where signing is required (XBAP's, GAC), where security is less of an issue.

When you think about it:

  * Magellan is OSS and not backed by a company
  * Project contributors could include all kinds of shady characters - you wouldn't know
  * I have a low tolerance for torture, and can't resist a good bribe

Therefore, my keeping the private key secret or publishing it on the web makes no difference, since you should assume I sold it to the Russian Mafia anyway.

# What this means #

If your `NuclearPowerPlant.exe` is signed and you really, really want to trust that the Magellan assembly you're using hasn't been tampered with:

  * Download the code
  * Inspect it
  * Compile and sign it yourself

Then it's up to you to keep the private key secure, not a faceless guy on the interwebs.