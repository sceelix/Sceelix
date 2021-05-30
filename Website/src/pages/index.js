import React from 'react';
import clsx from 'clsx';
import Layout from '@theme/Layout';
import Link from '@docusaurus/Link';
import useDocusaurusContext from '@docusaurus/useDocusaurusContext';
import styles from './index.module.css';
import HomepageFeatures from '../components/HomepageFeatures';
import LightBoxImage from '../components/LightBoxImage'

const FeatureList = [
  {
    title: 'What is Sceelix?',
    image: 'img/main/Lights and House Numbers.jpg',
    description: (
      <>
        Sceelix is a software for automating 2D/3D content creation using algorithms, rules, and mathematical models, a practice called procedural generation.
      </>
    ),
  },
  {
    title: 'What is it good for?',
    image: 'img/main/Voxels.jpg',
    description: (
      <>
        Content creation is an expensive process, and can be sometimes tedious and error-prone. By introducing automation, Sceelix allows for more speed, consistency and flexibility in its production.
      </>
    ),
  },
  {
    title: 'What can I create with it?',
    image: 'img/main/Vegetation.jpg',
    description: (
      <>
        Just about anything. Sceelix is especially useful to create large scenes that feature reproducible patterns, such as cities, forests, mountains, etc.
      </>
    ),
  },
  {
    title: 'How does it work?',
    image: 'img/main/ParkingLotGraphExample.jpg',
    description: (
      <>
        Sceelix uses a visual language that allows operations to be chained, defining a content creation process. Operations create, load, transform or export data and can use randomness and external data to produce infinite variations.
      </>
    ),
  },
];

const UniquePoints = [
  {
    title: 'Expressive',
    image: 'img/main/GraphSample.jpg',
    description: (
      <>
        Sceelix features a distinct visual language design that allows for high-level, data-focused control.
      </>
    ),
  },
  {
    title: 'Multi-purpose',
    image: 'img/main/Physics.jpg',
    description: (
      <>
        Sceelix can control and produce different kinds of content (3D meshes, textures, heightmaps, game objects…) within a single pipeline.
      </>
    ),
  },
  {
    title: 'Extensible',
    image: 'img/main/APISampleCode.png',
    description: (
      <>
        Through a simple and carefully-designed C# API, anyone can develop new features in Sceelix: add more algorithms and data types; define new import and export formats, build UI extensions and much more!
      </>
    ),
  },
  {
    title: 'Integratable',
    image: 'img/main/SceelixToUnity.png',
    description: (
      <>
        Either by direct library import or through interprocess communication, Sceelix's functions can interact with your application or favorite design tools.
      </>
    ),
  },
];


const Industries = [
  {
    description: 'Game Design, VR',
    image: 'img/main/GamingIcon.png'
  },
  {
    description: 'Simulation',
    image: 'img/main/SimulationIcon.png'
  },
  {
    description: 'Film and Media Production',
    image: 'img/main/FilmIcon.png'
  },
  {
    description: 'Architecture and Urban Planning',
    image: 'img/main/UrbanIcon.png'
  },
  {
    description: 'Education',
    image: 'img/main/EducationIcon.png'
  },
  {
    description: 'And more...',
    image: 'img/main/ListIcon.png'
  }
];

function HomepageHeader() {

  return (
    <header className={clsx('hero hero--dark', styles.heroBanner)} style={
      {
        backgroundImage: `url("./img/main/CityHDRVignetteTop.jpg")`,
        backgroundRepeat: 'no-repeat',
        backgroundSize: 'cover',
        backgroundPosition: 'center'
      }}>

      <div className="container padding-vert--xl text--left">
        <div className="row">
          <div className="col col--6" >
            <div className="padding-vert--lg text--center">
              <h1 className="hero__title hero_text">The 3D<br />Procedural<br />Engine</h1>
              <Link className="button button--secondary button--lg" to="https://github.com/Sceelix/Sceelix/releases/latest">Download</Link>
            </div>
          </div>
          <div className="col col--6">
            <div className="text--center">
              <LightBoxImage image="img/main/SceelixVideoStill.png"
              videoLink="https://www.youtube.com/watch?v=ViberNfXBGg"
              alt="Sceelix Intro Video"/>
            </div>
          </div>
        </div>
      </div>
    </header>
  );
}

export default function Home() {
  const { siteConfig } = useDocusaurusContext();
  return (
    <Layout
      title={`Home`}
      description="Sceelix Home">
      <HomepageHeader />
      <main>
        <HomepageFeatures features={FeatureList} size={6} />
      </main>

      <div className="container text--center padding-top--lg">
        <h1>Who is this software for?</h1>
      </div>
      <HomepageFeatures features={Industries} size={2} />

      <div className="container text--center padding-top--lg">
        <h1>How is Sceelix Unique?</h1>
      </div>
      <HomepageFeatures features={UniquePoints} size={3} />

      <div className="container text--center padding--lg">
        <h1>Other questions?</h1>
        <p>Check out the <a href="docs/Introduction/FAQ">FAQ</a>, <a href="docs/Introduction/WhatIsSceelix">Documentation</a> or the <a href="https://github.com/Sceelix/Sceelix/discussions">discussions!</a></p>
      </div>

    </Layout>
  );
}
