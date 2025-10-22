import { CSSProperties, MouseEventHandler, ReactNode } from "react";

// Props for Table
interface TableProps {
  children: ReactNode; // Table content (thead, tbody, etc.)
  className?: string; // Optional className for styling
}

// Props for TableHeader
interface TableHeaderProps {
  children: ReactNode; // Header row(s)
  className?: string; // Optional className for styling
}

// Props for TableBody
interface TableBodyProps {
  children: ReactNode; // Body row(s)
  className?: string; // Optional className for styling
}

// Props for TableRow
interface TableRowProps {
  children: ReactNode; // Cells (th or td)
  className?: string; // Optional className for styling
  style?: CSSProperties;
  onClick?: MouseEventHandler<HTMLTableRowElement>; 
}

// Props for TableCell
interface TableCellProps {
  readonly children?: ReactNode; // Cell content
  readonly isHeader?: boolean; // If true, renders as <th>, otherwise <td>
  readonly className?: string; // Optional className for styling
  readonly cellSpan?: number;
  readonly rowSpan?: number;
}

// Table Component
const Table: React.FC<TableProps> = ({ children, className }) => {
  return <table className={`min-w-full  ${className}`}>{children}</table>;
};

// TableHeader Component
const TableHeader: React.FC<TableHeaderProps> = ({ children, className }) => {
  return <thead className={className}>{children}</thead>;
};

// TableBody Component
const TableBody: React.FC<TableBodyProps> = ({ children, className }) => {
  return <tbody className={className}>{children}</tbody>;
};

// TableRow Component
const TableRow: React.FC<TableRowProps> = ({ children, className, style, onClick }) => {
  return <tr className={className} style={style} onClick={onClick}>{children}</tr>;
};

// TableCell Component
const TableCell: React.FC<TableCellProps> = ({
  children,
  isHeader = false,
  className,
  cellSpan,
  rowSpan
}) => {
  const CellTag = isHeader ? "th" : "td";
  return <CellTag className={className} colSpan={cellSpan} rowSpan={rowSpan}>{children}</CellTag>;
};

export { Table, TableHeader, TableBody, TableRow, TableCell };
