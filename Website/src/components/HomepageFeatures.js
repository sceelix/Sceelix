import React from 'react';
import clsx from 'clsx';
import styles from './HomepageFeatures.module.css';



function Feature({size, image, title, description}) {
  return (
    <div className={clsx('col col--' + size)}>
      <div className="text--center">
        <img alt={title} src={image} />
      </div>
      <div className="text--center padding-horiz--md">
        <h3>{title}</h3>
        <p>{description}</p>
      </div>
    </div>
  );
}

export default function HomepageFeatures({features, size}) {
  return (
    <section className={styles.features}>
      <div className="container">
        <div className="row">
          {features.map((props, idx) => (
            <Feature key={idx} size={size} {...props} />
          ))}
        </div>
      </div>
    </section>
  );
}
