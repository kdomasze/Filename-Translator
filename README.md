# Filename Translator (FnT)

Command utility to translate filenames from their source language to a desired language.

## What does it do?

After running from the command line, this utility will rename all the files within the current working directory from their source language (default: auto-detect) to the specified target language (default: english).

An example use-case for this is to translate the names of songs ripped from an imported foreign album.

## How does it work?

By utilizing the Google Translate AJAX API, you can send an http request of the text you wish to translate, and recieve back a formatted text file.

In order to limit the number of requests made to Google's servers, all the filenames in the current folder are appended to a single request. This also speeds up the execution time as FnT has to make a single request rather than make one request for every file.

## Getting Started

These instructions will get you a copy of the project up and running on your local machine for development.

### Prerequisites

The project was created using Visual Studio 2017 Version 15.3.4.

### Instructions for Building

Pull the project and open the solution in Visual Studio. It should be ready for compilation out of the box.

## Authors

* **Kyle Domaszewicz** - [kdomasze](https://github.com/kdomasze)

## License

This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details.
