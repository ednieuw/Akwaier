import AkwaierKern
import Foundation

// Draait alle proeven en drukt het verslag af. Sluit af met code 0 als alles
// goed is, anders met het aantal mislukte controles.
//     swift run AkwaierZelftest
let uitslag = Zelftest.draai()
print(uitslag.verslag)
exit(uitslag.goed ? 0 : 1)
