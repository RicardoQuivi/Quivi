// src/hooks/useExample.ts
import { useState } from "react";
var useExample = (initial) => {
  const [count, setCount] = useState(initial);
  const increment = () => setCount((c) => c + 2);
  return {
    count,
    increment
  };
};
export {
  useExample
};
//# sourceMappingURL=index.mjs.map