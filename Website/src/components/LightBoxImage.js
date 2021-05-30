import React, { useState } from 'react';
import FsLightbox from 'fslightbox-react';

function LightBoxImage({image, videoLink, alt}) {
    const [toggler, setToggler] = useState(false);

    return (
        <>
            <img style={{cursor:'pointer'}}
                onClick={() => setToggler(!toggler)}
                className={"shadow--tl"}
                alt={alt}
                src={image}
              />
            <FsLightbox
                toggler={toggler}
                sources={[
                    videoLink
                ]}
            />
        </>
    );
}

export default LightBoxImage;