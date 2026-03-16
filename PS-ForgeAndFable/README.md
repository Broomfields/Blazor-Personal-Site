
# Shaun's Personal Site - Blazor (Server) Application

Welcome to my personal site, built using Blazor (Server). This project showcases my work, skills, and interests as a developer. This README provides an overview of the application, instructions for setup, and details about each component within the site.

## Table of Contents
- [Overview](#overview)
- [Features](#features)
- [Technology Stack](#technology-stack)
- [Setup](#setup)
- [Project Structure](#project-structure)
- [Components](#components)
- [Usage](#usage)
- [Contributing](#contributing)
- [License](#license)

## Overview
This site is my personal portal—a space to introduce myself, showcase my skills, and centralise access to my online projects. Throughout my career, most of my programming has been proprietary work for companies, with limited opportunities to create a personal portfolio. This site is a step towards changing that: it’s both a launchpad and a canvas where I can explore personal projects, refine my skills, and build something I can proudly present. The single-page layout is modular and adaptable, making it easy to update as I grow as a developer and take on new challenges.

## Features
- **Dynamic and Responsive Design**: Built for seamless viewing across devices.
- **Modular Components**: Each section is a Razor component, improving maintainability and reusability.
- **Downloadable Resume**: Visitors can view and download my resume in PDF format.
- **Contact Form**: Built-in contact form for easy communication.
- **Social Media Links**: Direct links to my social profiles.

## Technology Stack
- **Framework**: [.NET 8](https://dotnet.microsoft.com/) with Blazor (Server)
- **Language**: C#
- **Database**: SQLite (if required for future features)
- **Styling**: CSS3
- **Version Control**: Git

## Setup
1. **Clone the repository**:
   ```bash
   git clone https://github.com/broomfields/blazor-personal-site.git
   cd personal site
   ```

2. **Install .NET 8 SDK** if not already installed:
   - [Download .NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

3. **Run the application**:
   ```bash
   dotnet run
   ```

4. **Open the application**:
   Navigate to `https://localhost:5001` (or the URL provided in your terminal) in your browser to view the personal site.

## Project Structure
The project follows a modular structure where each section of the personal site is a Razor component. This setup allows for easy maintenance and expansion. Key directories include:

- **Pages/**: Main pages and routing.
- **Components/**: Contains each section of the personal site as a standalone Razor component.
- **wwwroot/**: Static assets (CSS, images, PDF resume).
- **Data/**: If database integration is added, models and data services will reside here.

## Components
Each section of the personal site is encapsulated within its own Razor component for clarity and modularity:

1. **Hero Component**: Introduction with a brief bio and photo.
2. **About Me Component**: Overview of my professional background and interests.
3. **Projects Component**: Displays a selection of my best work with links.
4. **Skills Component**: Lists my technical skills with visual icons.
5. **Testimonials Component**: Includes quotes from colleagues or clients.
6. **Contact Component**: A form for visitors to reach out.
7. **Socials Component**: Links to my social media profiles.
8. **Hobbies Component**: Shares a bit about my personal interests.
9. **Resume Component**: Provides access to my resume with a PDF download option.

## Usage
After running the project, you can view and navigate through each section to explore my background, skills, projects, and interests. You can also download my resume and use the contact form to reach out.

## Contributing
This is a personal project, but contributions are welcome for suggestions or improvements. Please open an issue or submit a pull request.

## License
This project is open source and available under the MIT License. See the [LICENSE](LICENSE) file for more details.
