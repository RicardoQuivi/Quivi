import { Box } from "@mui/material";

export const GridShape = () => {
  return (
    <>
      <Box
        position="absolute"
        top={0}
        right={0}
        zIndex={-1}
        width="100%"
        sx={{
          maxWidth: { xs: '250px', xl: '450px' },
        }}
      >
        <img src="/images/shape/grid-01.svg" alt="grid" />
      </Box>
      <Box
        position="absolute"
        bottom={0}
        left={0}
        zIndex={-1}
        width="100%"
        sx={{
          maxWidth: { xs: '250px', xl: '450px' },
          transform: 'rotate(180deg)',
        }}
      >
        <img src="/images/shape/grid-01.svg" alt="grid" />
      </Box>
    </>
  );
}