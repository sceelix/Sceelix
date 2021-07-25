import React from 'react';
import Tree from 'react-animated-tree-v2';

const close = (props) => (
<svg {...props} x="0px" y="0px" viewBox="0 0 416 416">
    <g>
        <path
            d="M213.333,106.667c-58.88,0-106.667,47.787-106.667,106.667S154.453,320,213.333,320S320,272.213,320,213.333
S272.213,106.667,213.333,106.667z"/>
        <path
            d="M213.333,0C95.467,0,0,95.467,0,213.333s95.467,213.333,213.333,213.333S426.667,331.2,426.667,213.333
S331.2,0,213.333,0z M213.333,384c-94.293,0-170.667-76.373-170.667-170.667S119.04,42.667,213.333,42.667
S384,119.04,384,213.333S307.627,384,213.333,384z"/>
    </g>
</svg>
);

// Using react animated Tree
// https://github.com/adityasonel/react-animated-tree-v2 (sample: https://codesandbox.io/embed/react-animated-tree-v2-custom-icons-mz23x)
// derived from https://www.npmjs.com/package/react-animated-tree (https://codesandbox.io/embed/rrw7mrknyp)

function renderPorts(ports) {
    return ports ? <i>[Adds ports {ports.map(x => <a href={'#port' + x}>{x}</a>).reduce((prev, curr) => [prev, ', ', curr])}]</i> : null;
}


export const ParameterTree = ({ children, name, type, description, open, ports}) => (
<Tree icons={{ closeIcon: close }}
      content={<span style={{ whiteSpace: 'normal' }}><b>{name}</b> [<i>{type}</i>] {renderPorts(ports)}
            <br />{description}</span>}
      children={children}
      open={open}/>
);