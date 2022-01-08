/** @type {import('@docusaurus/types').DocusaurusConfig} */
module.exports = {
  title: 'Sceelix',
  tagline: 'The 3D Procedural Engine',
  url: 'https://sceelix.github.io',
  baseUrl: '/',
  onBrokenLinks: 'throw',
  onBrokenMarkdownLinks: 'warn',
  favicon: 'img/favicon.ico',
  organizationName: 'sceelix', // Usually your GitHub org/user name.
  projectName: 'sceelix', // Usually your repo name.
  themeConfig: {
    navbar: {
      title: 'Sceelix',
      logo: {
        alt: 'Sceelix Logo',
        src: 'img/SceelixLogo.png',
      },

      items: [
        {
          to: 'https://github.com/Sceelix/Sceelix/releases/latest',
          position: 'left',
          label: 'Download',
        },
        {
          to: '/docs/Introduction/AboutSceelix',
          position: 'left',
          label: 'Documentation',
        },
        {
          to: '/blog',
          label: 'What\'s New',
          position: 'left'
        },
        {
          href: 'https://github.com/Sceelix/Sceelix',
          position: 'right',
          className: 'cornerIcon githubIcon',
          'aria-label': 'GitHub repository',
        },
        {
          href: 'https://www.youtube.com/channel/UC8ONWvQNJ01JroGpCLJmrsQ/feed',
          position: 'right',
          className: 'cornerIcon youtubeIcon'
        },
        {
          href: 'https://www.facebook.com/sceelix',
          position: 'right',
          className: 'cornerIcon facebookIcon'
        },
        {
          href: 'https://twitter.com/sceelix',
          position: 'right',
          className: 'cornerIcon twitterIcon'
        }
      ],
    },
      algolia: {
        appId: '#{ALGOLIA_APP_ID}#',
        apiKey: '#{ALGOLIA_API_KEY}#',
        indexName: 'sceelix',

        // Optional: see doc section below
        contextualSearch: true
    },
    colorMode: {
      // "light" | "dark"
      defaultMode: 'dark',

      // Hides the switch in the navbar
      // Useful if you want to support a single color mode
      disableSwitch: true
    },
    footer: {
      style: 'dark',
      links: [
        {
          title: 'Documentation',
          items: [
            {
              label: 'About Sceelix',
              to: '/docs/Introduction/AboutSceelix',
            },
            {
              label: 'Working with Sceelix',
              to: '/docs/Working%20With%20Sceelix/Interface',
            },
            {
              label: 'Nodes',
              to: '/docs/Nodes/NodeOverview',
            },
            {
              label: 'SDK',
              to: '/docs/SDK/Introduction',
            },
          ],
        },
        {
          title: 'Community',
          items: [
            {
              label: 'Facebook',
              href: 'https://www.facebook.com/sceelix',
            },
            {
              label: 'Youtube',
              href: 'https://www.youtube.com/channel/UC8ONWvQNJ01JroGpCLJmrsQ/feed',
            },
            {
              label: 'Twitter',
              href: 'https://twitter.com/sceelix',
            },
          ],
        },
        {
          title: 'More',
          items: [
            {
              label: 'Blog',
              to: '/blog',
            },
            {
              label: 'GitHub',
              href: 'https://github.com/Sceelix/Sceelix',
            },
          ],
        },
      ],
      copyright: `Copyright © ${new Date().getFullYear()} Sceelix. Built with Docusaurus.`,
    },
  },
  presets: [
    [
      '@docusaurus/preset-classic',
      {
        docs: {
          sidebarPath: require.resolve('./sidebars.js'),
          // Please change this to your repo.
          editUrl:
            'https://github.com/Sceelix/Sceelix/edit/master/website/',
        },
        blog: {
          showReadingTime: true,
          // Please change this to your repo.
          editUrl:
            'https://github.com/Sceelix/Sceelix/edit/master/website/blog/',
        },
        theme: {
          customCss: require.resolve('./src/css/custom.css'),
        },
      },
    ],
  ],
  plugins: [
    [
      '@docusaurus/plugin-client-redirects',
      {
        redirects: [
          {
            from: ['/docs/'],
            to: '/docs/Introduction/AboutSceelix',
          }
        ],
      },
    ],
  ]
};
