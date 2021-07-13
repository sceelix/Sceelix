import React from 'react';
import Tree from 'react-animated-tree-v2';

const plus = (props) => (
<svg {...props} x="0px" y="0px" viewBox="0 0 408 408">
    <path d="M372,88.661H206.32l-33-39.24c-0.985-1.184-2.461-1.848-4-1.8H36c-19.956,0.198-36.023,16.443-36,36.4v240
c-0.001,19.941,16.06,36.163,36,36.36h336c19.94-0.197,36.001-16.419,36-36.36v-199C408.001,105.08,391.94,88.859,372,88.661z"
    />
    </svg>
);

const minus = (props) => (
<svg {...props} x="0px" y="0px" viewBox="0 0 408 408">
    <g>
    <g>
    <path
d="M367.731,112.653H40.291c-22.269,0.132-40.258,18.21-40.28,40.48c-0.015,0.226-0.015,0.454,0,0.68l23.4,174.6
c0.284,22.16,18.318,39.98,40.48,40h280.4c22.161-0.02,40.196-17.84,40.48-40l23.24-174.6c0.015-0.226,0.015-0.454,0-0.68
C407.99,130.863,390.001,112.785,367.731,112.653z"
    />
    </g>
    </g>
    <g>
    <g>
    <path
d="M337.851,72.333h-131.52l-26-30.92c-0.985-1.184-2.461-1.848-4-1.8H70.171c-16.559,0.022-29.978,13.441-30,30v28.84h10
h317.4C365.624,83.521,352.909,72.347,337.851,72.333z"
    />
    </g>
    </g>
    </svg>
);

const close = (props) => (
<svg {...props} x="0px" y="0px" viewBox="0 0 416 416">
    <g>
    <path
d="M213.333,106.667c-58.88,0-106.667,47.787-106.667,106.667S154.453,320,213.333,320S320,272.213,320,213.333
S272.213,106.667,213.333,106.667z"
    />
    <path
d="M213.333,0C95.467,0,0,95.467,0,213.333s95.467,213.333,213.333,213.333S426.667,331.2,426.667,213.333
S331.2,0,213.333,0z M213.333,384c-94.293,0-170.667-76.373-170.667-170.667S119.04,42.667,213.333,42.667
S384,119.04,384,213.333S307.627,384,213.333,384z"
    />
    </g>
    </svg>
);

// Using react animated Tree
// https://github.com/adityasonel/react-animated-tree-v2 (sample: https://codesandbox.io/embed/react-animated-tree-v2-custom-icons-mz23x) 
// derived from https://www.npmjs.com/package/react-animated-tree (https://codesandbox.io/embed/rrw7mrknyp)


export const ParameterTree = ({ children, name, type, description, open }) => (
    <Tree icons={{ plusIcon: plus, minusIcon: minus, closeIcon: close }}
        content={<span style={{ whiteSpace: 'normal' }}><b>{name}</b> [<i>{type}</i>]<br />{description}</span>} children={children} open={open}/>
);