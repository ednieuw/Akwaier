// swift-tools-version: 5.9
import PackageDescription

// De spelkern van Akwaier, los van elk scherm. De SwiftUI-app voor iPad en Mac
// komt hier straks bovenop; deze package bevat alleen de regels en een zelftest.
let package = Package(
    name: "AkwaierKern",
    platforms: [.macOS(.v13), .iOS(.v16)],
    products: [
        .library(name: "AkwaierKern", targets: ["AkwaierKern"]),
        .executable(name: "AkwaierZelftest", targets: ["AkwaierZelftest"]),
    ],
    targets: [
        .target(name: "AkwaierKern"),
        .executableTarget(name: "AkwaierZelftest", dependencies: ["AkwaierKern"]),
        .testTarget(name: "AkwaierKernTests", dependencies: ["AkwaierKern"]),
    ]
)
